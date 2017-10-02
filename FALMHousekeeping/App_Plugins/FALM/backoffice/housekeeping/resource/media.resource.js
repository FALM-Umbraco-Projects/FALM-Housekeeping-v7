angular.module('umbraco.resources')
    .factory('hkMediaResource', function($http) {
        //the factory object returned
        return {
            //this calls the Api Controller we setup earlier
            getMediaToDelete: function (userLocale) {
                return $http({
                    method: 'GET',
                    url: 'FALMHousekeeping/HkMediaApi/GetMediaToDelete',
                    params: { userLocale: userLocale }
                });
            },
            //this calls the Api Controller and execute deleteSelectedUsers(listufusers) method
            postDeleteMediaOrphans: function (mediaOrphansToDelete) {
                return $http({
                    method: 'POST',
                    url: 'FALMHousekeeping/HkMediaApi/PostDeleteMediaOrphans',
                    data: angular.toJson(mediaOrphansToDelete)
                });
            }
        };
    });