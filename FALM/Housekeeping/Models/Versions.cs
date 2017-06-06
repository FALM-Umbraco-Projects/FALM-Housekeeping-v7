// SYSTEM
using System;
using System.Collections.Generic;

namespace FALM.Housekeeping.Models
{
    public class VersionsModel
    {
        public List<CurrentPublishedVersionModel>   ListVersions                { get; set; }
    }

    public class CurrentPublishedVersionModel
    {
        public int                                  NodeId                      { get; set; }
        public string                               NodeName                    { get; set; }
        public string                               VersionGUID                 { get; set; }
        public DateTime                             CurrentPublishedVersionDate { get; set; }
        public List<OtherVersions>                  AllNodeVersions             { get; set; }
    }

    public class OtherVersions
    {
        public string                               VersionGUID                 { get; set; }
        public DateTime                             VersionDate                 { get; set; }
    }
}