angular.module('umbraco.resources')
    .factory('hkUsersResource', function($http) {
        //the factory object returned
        return {
            //this calls the Api Controller we setup earlier
            getAllUsers: function () {
                return $http.get("FALMHousekeeping/HKUsersApi/GetAllUsers");
            },
            //this calls the Api Controller and execute deleteSelectedUsers(listufusers) method
            deleteSelectedUsers: function (selectedUsers) {
                return $http({
                    method: 'POST',
                    url: 'FALMHousekeeping/HKUsersApi/PostDeleteSelectedUsers',
                    data: angular.toJson(selectedUsers)
                });
            }
        };
    });