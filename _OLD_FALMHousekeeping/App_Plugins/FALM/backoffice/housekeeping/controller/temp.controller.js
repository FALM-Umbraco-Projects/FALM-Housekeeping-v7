'use strict';
(function () {
    // Create Edit controller
    function TempCleanupController($route, $scope, $routeParams, appState, treeService, navigationService, hkTempResource, userService, notificationsService, localizationService, eventsService) {
        // Set a property on the scope equal to the current route id
        $scope.id = $routeParams.id;

        $scope.reloadRoute = function () {
            $route.reload();
        };

        $scope.currentUserLanguage = "en-GB";

        userService.getCurrentUser().then(function (user) {
            $scope.currentUserLanguage = user.locale;
        });

        $scope.showSearchPanel = false;
        $scope.showLoader = false;
        $scope.showDeletionLoader = false;

        // GET - VIEW TEMP DIRECTORIES CONTENT VIA hkTempResource
        hkTempResource.getTempContent().then(function (response) {
            $scope.showResultPanel = false;
            $scope.showDeletionLoader = false;
            $scope.showLoader = true;
            $scope.tempContentList = response.data;
            $scope.showLoader = false;
            $scope.showResultPanel = true;
        });

        // Select and Unselect all TEMP entries
        $scope.checkAll = function (obj) {
            angular.forEach($scope.tempContentList, function (tempItem) {
                tempItem.Selected = obj.selectAll;
            });
        };

        // Check if at least one user is selected
        $scope.isAtLeastOneEntrySelected = function () {
            for (var t in $scope.tempContentList) {
                var ti = $scope.tempContentList[t];
                if (ti.Selected)
                    return true;
            }
            return false;
        };

        // Empty Content Recycle Bin
        $scope.emptyTempDirectorySuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_CacheTempManager.Temp.EmptyTempDirectorySuccessHeadline").then(function (value) {
            $scope.emptyTempDirectorySuccessNotification.headline = value;
        });
        localizationService.localize("FALM_CacheTempManager.Temp.EmptyTempDirectorySuccessMessage").then(function (value) {
            $scope.emptyTempDirectorySuccessNotification.message = value;
        });

        $scope.emptyTempDirectoryErrorNotification = {
            'type': 'error',
            'sticky': true
        };
        localizationService.localize("FALM_CacheTempManager.Temp.EmptyTempDirectoryErrorHeadline").then(function (value) {
            $scope.emptyTempDirectoryErrorNotification.headline = value;
        });
        localizationService.localize("FALM_CacheTempManager.Temp.EmptyTempDirectoryErrorMessage").then(function (value) {
            $scope.emptyTempDirectoryErrorNotification.message = value;
        });

        localizationService.localize("FALM_CacheTempManager.Temp.ConfirmEmptyTempDirectoryMessage").then(function (value) {
            $scope.ConfirmEmptyTempDirectoryMessage = value;
        });

        // Post temp directory list to delete via hkTempResource
        $scope.emptySelectedTempDirectories = function (selectedTempItems) {
            if (confirm($scope.ConfirmEmptyTempDirectoryMessage)) {
                hkTempResource.emptySelectedTempDirectories(selectedTempItems).then(function (response) {
                    if (response.data === "true") {
                        notificationsService.add($scope.emptyTempDirectorySuccessNotification);
                    }
                    else {
                        notificationsService.add($scope.emptyTempDirectoryErrorNotification);
                    }
                    $scope.showDeletionLoader = false;
                    $route.reload();
                });
            }
        };

        // TREE NODE HIGHLIGHT
        var activeNode = appState.getTreeState("selectedNode");
        if (activeNode) {
            var activeNodePath = treeService.getPath(activeNode).join();
            navigationService.syncTree({ tree: $routeParams.tree, path: activeNodePath, forceReload: false, activate: true });
        } else {
            navigationService.syncTree({ tree: $routeParams.tree, path: ["-1", "cachetemp", $routeParams.id], forceReload: false, activate: true });
        }
    }

    // Register the controller
    angular.module("umbraco").controller("FALMHousekeepingTempCleanupController", TempCleanupController);
})();