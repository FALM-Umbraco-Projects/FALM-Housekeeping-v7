angular.module('umbraco.resources').factory('hkRecycleBinResource', function($http) {
    //the factory object returned
    return {
        //calls the Api Controller we setup earlier
        getAllItemsInRecycleBins: function () {
            return $http.get("FALMHousekeeping/HkRecycleBinApi/GetAllItemsInRecycleBins");
        },
        //calls the Api Controller and execute GetCreateFALMServicePage() method
        createServicePage: function () {
            return $http.get("FALMHousekeeping/HkRecycleBinApi/GetCreateServicePage");
        },
        //calls the Api Controller and execute PostEmptyMediaRecycleBin() method
        emptyContentRecycleBin: function () {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/HkRecycleBinApi/PostEmptyContentRecycleBin'
            });
        },
        //calls the Api Controller and execute PostEmptyMediaRecycleBin() method
        emptyMediaRecycleBin: function () {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/HkRecycleBinApi/PostEmptyMediaRecycleBin'
            });
        },
        //calls the Api Controller and execute PostEmptyBothRecycleBins() method
        emptyBothRecycleBins: function () {
            return $http({
                method: 'POST',
                url: 'FALMHousekeeping/HkRecycleBinApi/PostEmptyBothRecycleBins'
            });
        }
    };
});