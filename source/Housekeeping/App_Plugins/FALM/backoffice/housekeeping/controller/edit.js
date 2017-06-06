'use strict';
(function () {
    // Create Edit controller
    function EditController($scope, $routeParams) {
        // Set a property on the scope equal to the current route id
        $scope.id       = $routeParams.id;
    };

    // Register the controller
    angular.module("umbraco").controller("FALMHousekeepingEditController", EditController);
})();