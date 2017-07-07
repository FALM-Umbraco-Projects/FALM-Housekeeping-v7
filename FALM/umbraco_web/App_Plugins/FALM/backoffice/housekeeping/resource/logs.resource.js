angular.module('umbraco.resources').factory('hkLogsResource', function($http) {
    //the factory object returned
    return {
        //this calls the Api Controller and execute GetDBLogs()
        getDBLogs: function () {
            return $http.get("FALMHousekeeping/LogsApi/GetDBLogs");
        },
        //this calls the Api Controller and execute PostDeleteDBLogs(filteredLogs) method
        deleteFilteredDBLogs: function (filteredLogs) {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/LogsApi/PostDeleteDBLogs',
                data: angular.toJson(filteredLogs)
            });
        },
        //this calls the Api Controller and execute GetTraceLogs()
        getTraceLogs: function (id) {
            return $http({
                method: 'GET',
                url: 'FALMHousekeeping/LogsApi/GetTraceLogs',
                params: { filename: id }
            });
        }
    };
});