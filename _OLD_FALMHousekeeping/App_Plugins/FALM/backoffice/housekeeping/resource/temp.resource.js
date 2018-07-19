angular.module('umbraco.resources').factory('hkTempResource', function ($http) {
    //the factory object returned
    return {
        //Get Temp Content
        getTempContent: function () {
            return $http.get("FALMHousekeeping/HkTempApi/GetTempContent");
        },
        //this calls the Api Controller and execute emptySelectedTempDirectories(listoftempdirectories) method
        emptySelectedTempDirectories: function (selectedTempItems) {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/HkTempApi/PostEmptySelectedTempDirectories',
                data: angular.toJson(selectedTempItems)
            });
        }
    };
});