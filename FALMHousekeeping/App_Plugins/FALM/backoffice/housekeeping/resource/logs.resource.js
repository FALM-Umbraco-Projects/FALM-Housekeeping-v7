angular.module('umbraco.resources').factory('hkLogsResource', function ($http) {
    //the factory object returned
    return {
        // ################################
        // # DB LOGS RESOURCES (Logs API) #
        // ################################
        // Get DB Logs
        getDBLogs: function (search, itemsPerPage, pageNumber) {
            return $http.get('FALMHousekeeping/HkLogsApi/GetDBLogs', { params: { search: search, itemsPerPage: itemsPerPage, pageNumber: pageNumber } });
        },
        // Delete Filtered Logs
        deleteFilteredDBLogs: function (filteredLogs) {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/HkLogsApi/PostDeleteDBLogs',
                data: angular.toJson(filteredLogs)
            });
        },
        // Delete DB Logs By Date
        deleteDBLogsBeforeMonths: function () {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/HkLogsApi/PostDeleteDBLogsBeforeMonths'
            });
        },

        // ###################################
        // # TRACE LOGS RESOURCES (Logs API) #
        // ###################################
        // Get Trace Logs
        getTraceLogs: function (id, itemsPerPage, pageNumber) {
            return $http.get('FALMHousekeeping/HkLogsApi/GetTraceLogs', { params: { filename: id, itemsPerPage: itemsPerPage, pageNumber: pageNumber } });
        },
        // Get Filtered Trace Logs
        filterTraceLogs: function (id, allTraceLogs, allFilteredTraceLogs, search, previousSearch, itemsPerPage, pageNumber) {
            var params = { FileName: id, ListAllTraceLogs: allTraceLogs, ListAllFilteredTraceLogs: allFilteredTraceLogs, ListTraceLogs: null, Search: search, PreviousSearch: previousSearch, ItemsPerPage: itemsPerPage, CurrentPage: pageNumber };

            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/HkLogsApi/PostFilterTraceLogs',
                data: angular.toJson(params)
            });
        }
    };
});