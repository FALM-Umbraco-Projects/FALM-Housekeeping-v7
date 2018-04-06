angular.module('umbraco.resources').factory('hkCacheResource', function($http) {
    //the factory object returned
    return {
        //Get Cache Content
        getCacheContent: function () {
            return $http.get("FALMHousekeeping/HkCacheApi/GetCacheContent");
        },
        //Empty Cache Directory
        emptyCacheDirectory: function () {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/HkCacheApi/PostEmptyCacheDirectory'
            });
        },
        //Create the Cache Cleaner Service Page
        createServicePage: function () {
            return $http.get("FALMHousekeeping/HkCacheApi/GetCreateServicePage");
        }
    };
});