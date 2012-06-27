/// <reference path="../_references.js" />

function breadcrumbViewModel(text, url, active) {
    var self = this;
    self.text = text;
    self.url = url;
    self.active = active;
};

// View model which represents a file
function fileViewModel(name, path, content) {
    var self = this;

    self.name = name;
    self.path = path;
    self.content = content;    
};

// View model which represents a project item
function projectItemViewModel(name, path) {
    var self = this;

    self.name = name;
    self.path = path;
};

// View model which represents the project
function projectViewModel(name, username, path) {
    var self = this;

    self.name = ko.observable(name);
    self.username = ko.observable(username);
    self.path = ko.observable(path);
    self.directories = ko.observableArray();
    self.files = ko.observableArray();
    self.file = ko.observable();
    self.inProgress = ko.observable(true);

    // build the url from the path provided
    self.buildUrl = function (path) {
        var url = '#/open/' + self.username() + "/" + self.name();
        if (path) {
            url += path
        }

        return url;
    };

    // Build breadcrumbs from the current directory path
    self.breadcrumbs = ko.computed(function () {
        var paths = self.path().split("/");
        if (paths[paths.length - 1] == "") {
            paths.splice(paths.length - 1, 1);
        }
        var result = [];
        result.push(new breadcrumbViewModel("Root", self.buildUrl(), paths.length > 0));
        ko.utils.arrayForEach(paths, function (path) {
            var index = ko.utils.arrayIndexOf(paths, path);
            var pathTillIndex = "/";
            for (var i = 0; i <= index; i++) {
                pathTillIndex += (paths[i] + "/");
            }
            result.push(new breadcrumbViewModel(path, self.buildUrl(pathTillIndex), index < paths.length - 1));
        });

        return result;

    }, this);

    self.addDirectory = function (name, path) {
        self.directories.push(new projectItemViewModel(name, path));
    };

    self.addFile = function (name, path) {
        self.files.push(new projectItemViewModel(name, path));
    };

    // Load the project
    self.openProject = function () {
        var projectUrl = '/api/project/' + self.username() + '/' + self.name() + '/';
        var path = self.path();
        if (path) {
            projectUrl += path;
            // if filename, then request should end with '/' otherwise server reject request if it has forbidden extensions
            if (path.substring(path.length - 1) != '/') {
                projectUrl += '/';
            }
        }
        $.get(projectUrl, function (data) {
            if (data) {
                if (data.Type == 1) {

                    for (var i = 0; i < data.Items.length; i++) {
                        var item = data.Items[i];
                        if (item.Type == 1) {
                            self.addDirectory(item.Name, item.Path);
                        } else if (item.Type == 0) {
                            self.addFile(item.Name, item.Path);
                        } else {
                            // Do nothing
                        }
                    }
                } else if (data.Type == 0) {
                    self.file(new fileViewModel(data.Name, data.Path, data.Content));                    
                } else {
                    // Do nothing
                }

            } else {
                // Project not found
            }

            self.inProgress(false);
        });
    };
};

// View model which is used for collecting the project information
function projectInfoViewModel() {
    var self = this;

    self.url = ko.observable();
    self.username = ko.observable();
    self.name = ko.observable();
    self.hasError = ko.observable();

    // Validate project url url
    function validateUrl(url) {
        var uri = new Uri(url);
        var isValid = false;
        var path = uri.path();
        if (uri.host().indexOf('github.com') > -1 && path) {
            var matches = path.match(/^\/(.+)\/(.+)$/);
            if (matches) {
                self.username(matches[1]);
                self.name(matches[2]);
                isValid = true;
            }
        }
        self.hasError(!isValid);
    };

    // Subscribe to the url change to validate
    self.url.subscribe(validateUrl);    

    // Open the poject
    self.open = function () {
        location.hash = '#/open/' + self.username() + '/' + self.name();
    };

};

/// Application level view model class
function appViewModel() {
    var self = this;

    self.projectInfo = ko.observable();
    self.project = ko.observable();
    self.projectIsActive = ko.observable();
    self.projectStatus = ko.observable();
    self.findResult = ko.observableArray();

    self.projectHub = $.connection.projectHub;

    self.projectHub.projectStatus = function (data) {
        if (data) {
            // If completed
            if (data.Status == 2) {
                self.projectStatus('');
            } else {
                self.projectStatus(data.Message);
            }
        }
    };

    self.findReferences = function (kind, text, position) {
        var project = self.project();
        var currentFilePath = project.file().path;
        var findReferencesUrl = '/api/solution/findreferences';

        $.post(findReferencesUrl,
            {
                username: project.username(),
                project: project.name(),
                path: currentFilePath,
                text: text,
                position: position
            },
            function (result) {
                self.findResult(result);
            }
        );
    };
    
    // Routing handlers
    Sammy('#main', function () {

        // Matches route with format '#/open/{usename}/{projectname}/{path}
        this.get(/\#\/open\/([^\/]+)\/([^\/]+)\/(.*)/, function (context, username, project, path) {
            var projectVm = new projectViewModel(project, username, path || "");
            self.projectInfo(null);
            self.project(projectVm);
            projectVm.openProject();
            self.projectIsActive(true);
        });

        // Matches route with format '#/open/{usename}/{projectname}
        this.get('#/open/:username/:project', function (context) {
            this.app.runRoute('get', '#/open/' + context.params.username + '/' + context.params.project + '/');
        });

        // Matches the default route
        this.get('', function () {
            self.projectIsActive(false);
            self.project(null);
            self.projectInfo(new projectInfoViewModel());
        });

    }).run();

    $.connection.hub.start();
};

$(function () {

    var application = new appViewModel();
    ko.applyBindings(application);

    $.findReferences = function (kind, text, position) {
        if (application) {
            application.findReferences(kind, text, position);
        }
    };
});
