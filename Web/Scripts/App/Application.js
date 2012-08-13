/// <reference path="../_references.js" />

function breadcrumbViewModel(text, url, active) {
    var self = this;
    self.text = text;
    self.url = url;
    self.active = active;
};

// View model which represents a file
function fileViewModel(name, path, content, lineNumber) {
    var self = this;

    self.name = name;
    self.path = path;
    self.content = content;
    self.lineNumber = lineNumber;
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
    self.versionInfo = ko.observable();   

    self.projectGithubUrl = ko.computed(function () {
        return 'https://github.com/' + self.username() + '/' + self.name();
    });

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
    self.openProject = function (lineNumber) {
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

                if (data.DownloadedDate) {
                    self.versionInfo('Downloaded on ' + data.DownloadedDate);
                }

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
                    self.file(new fileViewModel(data.Name, data.Path, data.Content, lineNumber || 0));                    
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
    self.disableOpening = ko.observable(true);

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
        self.disableOpening(!isValid);
    };

    // Subscribe to the url change to validate
    self.url.subscribe(validateUrl);    

    // Open the poject
    self.open = function () {
        location.hash = '#/open/' + self.username() + '/' + self.name();
    };

};

/// Output window view model
function outPutWindowViewModel(title, content) {
    var self = this;

    self.show = ko.observable(true);
    self.title = title;
    self.content = content;

    self.close = function () {
        self.show(false);
    }
}

/// Find result window view model
function findResultWindowViewModel(title, items) {
    var self = this;

    self.show = ko.observable(true);
    self.title = title;
    self.items = items;

    self.close = function () {
        self.show(false);
    }
}

/// Application level view model class
function appViewModel() {
    var self = this;

    self.isReady = ko.observable(false);
    self.projectInfo = ko.observable();
    self.project = ko.observable();
    self.projectIsActive = ko.observable();
    self.projectStatus = ko.observable();
    self.findResult = ko.observable();
    self.output = ko.observable();

    self.projectHub = $.connection.projectHub;

    self.projectHub.projectStatus = function (data) {
        if (data) {            
            if (data.Status == 2) { // If completed
                self.projectStatus('');
            } else if (data.Status == 3) { // Error     
                self.projectStatus(data.Message);
                self.output(new outPutWindowViewModel(data.Message, data.DetailedMessage));
            }
            else {
                self.projectStatus(data.Message);
            }
        }
    };

    self.projectHub.findReferenceStatus = function (data) {
        if (data) {
            self.projectStatus(data);
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
                position: position,
                kind: kind
            },
            function (result) {
                self.findResult(new findResultWindowViewModel('Find result for "' + text + '"', result));
            }
        );
    };

    self.goToDefinition = function (fullyQualifiedName) {
        var project = self.project();
        var currentFilePath = project.file().path;
        var goToDefinitionUrl = '/api/solution/gotodefinition';

        $.post(goToDefinitionUrl,
            {
                username: project.username(),
                project: project.name(),
                path: currentFilePath,
                fullyQualifiedName: fullyQualifiedName,
            },
            function (result) {
                if (result) {
                    self.openFile(result);
                } else {
                    self.projectStatus("Couldn't find any implementation in the current solution.");
                }
            }
        );
    };

    self.openFile = function (result) {
        self.projectStatus('');
        if (result) {
            location.hash = '#/open/' + self.project().username() + '/' + self.project().name() + '/' + result.Path + '?line=' + result.Position;
        }
    };
    
    // Routing handlers
    Sammy('#main', function () {

        // Matches route with format '#/open/{usename}/{projectname}/{path}
        this.get(/\#\/open\/([^\/]+)\/([^\/]+)\/(.*)/, function (context, username, project, path) {
            var projectVm = new projectViewModel(project, username, path || "");
            self.projectInfo(null);
            self.project(projectVm);
            projectVm.openProject(context.params.line);
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

    $.connection.hub.start().done(function () {
        var projectConnectionId = $.connection.hub.id;
        $.cookie('ProjectConnectionId', projectConnectionId, {expires: 1});
    });
    self.isReady(true);
};

$(function () {

    // This is for getting rid of the UI flickering
    setTimeout(function () {
        $('#apploader').toggleClass('hide');
        $('#main').toggleClass('hide');
    }, 500);

    ko.bindingHandlers.scrollTo = {
        update: function (element, valueAccessor, allBindingsAccessor) {
            var currentElement = $(element);
            var value = ko.utils.unwrapObservable(valueAccessor()) * parseInt(currentElement.css('line-height').replace('px', ''));
            currentElement.parents('.scroll-y').animate({ scrollTop: value });
        }
    };

    var application = new appViewModel();
    ko.applyBindings(application);

    $.findReferences = function (kind, text, position) {
        if (application) {
            application.findReferences(kind, text, position);
        }
    };

    $.goToDefinition = function (fullyQualifiedName) {
        if (application) {
            application.goToDefinition(fullyQualifiedName);
        }
    };
});
