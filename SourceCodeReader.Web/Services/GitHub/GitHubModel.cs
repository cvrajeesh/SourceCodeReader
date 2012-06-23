using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestSharp.Serializers;

namespace SourceCodeReader.Web.Services.GitHub
{
    public class GitHubRepo
    {
        [SerializeAs(NameStyle = NameStyle.LowerCase)]
        public string Description { get; set; }

        [SerializeAs(NameStyle = NameStyle.LowerCase)]
        public string Master_Branch { get; set; }

        [SerializeAs(NameStyle = NameStyle.LowerCase)]
        public string Full_Name { get; set; }
    }

    public class GitHubItem
    {
        public GitHubItemLinks _links { get; set; }

        [SerializeAs(NameStyle = NameStyle.LowerCase)]
        public string Name { get; set; }

        [SerializeAs(NameStyle = NameStyle.LowerCase)]
        public string Path { get; set; }

        [SerializeAs(NameStyle = NameStyle.LowerCase)]
        public string SHA { get; set; }

        [SerializeAs(NameStyle = NameStyle.LowerCase)]
        public string Type { get; set; }
    }

    public class GitHubItemLinks
    {
        [SerializeAs(NameStyle = NameStyle.LowerCase)]
        public string Git { get; set; }

        [SerializeAs(NameStyle = NameStyle.LowerCase)]
        public string Self { get; set; }

        [SerializeAs(NameStyle = NameStyle.LowerCase)]
        public string Html { get; set; }
    }
}