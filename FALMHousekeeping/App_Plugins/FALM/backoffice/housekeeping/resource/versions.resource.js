angular.module('umbraco.resources').factory('hkVersionsResource', function($http) {
    //the factory object returned
    return {
        //this calls the Api Controller and execute GetPublishedNodes()
        getPublishedNodes: function (search, itemsPerPage, pageNumber) {
            return $http.get('FALMHousekeeping/HkVersionsApi/GetPublishedNodes', { params: { search: search, itemsPerPage: itemsPerPage, pageNumber: pageNumber } });
        },
        //this calls the Api Controller and execute GetVersionsByNodeId(nodeId)
        getVersionsByNodeId: function (publishedNodeId) {
            return $http({
                method: 'GET',
                url: 'FALMHousekeeping/HkVersionsApi/GetVersionsByNodeId',
                params: { publishedNodeId: publishedNodeId }
            });
        },
        //this calls the Api Controller and execute PostDeleteVersionsByNodeId(publishedNodeId) method
        deleteVersionsByNodeId: function (publishedNodeId, versionsToKeep) {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/HkVersionsApi/PostDeleteVersionsByNodeId',
                params: { publishedNodeId: publishedNodeId, versionsToKeep: versionsToKeep }
            });
        },
        //this calls the Api Controller and execute PostDeleteVersionsByCount(versionsToKeep) method
        deleteVersionsByCount: function (versionsToKeep) {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/HkVersionsApi/PostDeleteVersionsByCount',
                params: { versionsToKeep: versionsToKeep }
            });
        }
    };
});