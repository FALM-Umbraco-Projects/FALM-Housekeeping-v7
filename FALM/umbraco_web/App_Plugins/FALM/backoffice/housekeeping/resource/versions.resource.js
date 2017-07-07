angular.module('umbraco.resources').factory('hkVersionsResource', function($http) {
    //the factory object returned
    return {
        //this calls the Api Controller and execute GetPublishedNodes()
        getPublishedNodes: function () {
            return $http.get("FALMHousekeeping/VersionsApi/GetPublishedNodes");
        },
        //this calls the Api Controller and execute PostDeleteVersions(filteredVersions) method
        //deleteFilteredVersions: function (filteredVersions) {
        //    return $http({
        //        method: 'POST',
        //        url: 'FALMHousekeeping/VersionsApi/PostDeleteVersions',
        //        data: angular.toJson(filteredVersions)
        //    });
        //},
        //this calls the Api Controller and execute GetVersionsByNodeId(nodeId)
        getVersionsByNodeId: function (publishedNodeId) {
            return $http({
                method: 'GET',
                url: 'FALMHousekeeping/VersionsApi/GetVersionsByNodeId',
                params: { publishedNodeId: publishedNodeId }
            });
        },
        //this calls the Api Controller and execute PostDeleteVersionsByNodeId(publishedNodeId) method
        deleteVersionsByNodeId: function (publishedNodeId, versionsToKeep) {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/VersionsApi/PostDeleteVersionsByNodeId',
                params: { publishedNodeId: publishedNodeId, versionsToKeep: versionsToKeep }
            });
        },
        //this calls the Api Controller and execute PostDeleteVersionsByCount(versionsToKeep) method
        deleteVersionsByCount: function (versionsToKeep) {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/VersionsApi/PostDeleteVersionsByCount',
                params: { versionsToKeep: versionsToKeep }
            });
        }
    };
});