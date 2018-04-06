'use strict';
(function () {
    // Create Edit controller
    function CacheCleanupController($route, $scope, $routeParams, appState, treeService, navigationService, hkCacheResource, userService, notificationsService, localizationService, eventsService) {
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

        // GET - VIEW CACHE DIRECTORIES CONTENT VIA hkCacheResource
        hkCacheResource.getCacheContent().then(function (response) {
            $scope.showResultPanel = false;
            $scope.showDeletionLoader = false;
            $scope.showLoader = true;
            $scope.cacheContent = response.data;
            $scope.showLoader = false;
            $scope.showResultPanel = true;
        });

        // Empty Content Recycle Bin
        $scope.emptyCacheDirectorySuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_CacheTempManager.Cache.EmptyCacheDirectorySuccessHeadline").then(function (value) {
            $scope.emptyCacheDirectorySuccessNotification.headline = value;
        });
        localizationService.localize("FALM_CacheTempManager.Cache.EmptyCacheDirectorySuccessMessage").then(function (value) {
            $scope.emptyCacheDirectorySuccessNotification.message = value;
        });

        $scope.emptyCacheDirectoryErrorNotification = {
            'type': 'error',
            'sticky': true
        };
        localizationService.localize("FALM_CacheTempManager.Cache.EmptyCacheDirectoryErrorHeadline").then(function (value) {
            $scope.emptyCacheDirectoryErrorNotification.headline = value;
        });
        localizationService.localize("FALM_CacheTempManager.Cache.EmptyCacheDirectoryErrorMessage").then(function (value) {
            $scope.emptyCacheDirectoryErrorNotification.message = value;
        });

        localizationService.localize("FALM_CacheTempManager.Cache.ConfirmEmptyCacheDirectoryMessage").then(function (value) {
            $scope.ConfirmEmptyCacheDirectoryMessage = value;
        });

        $scope.emptyCacheDirectory = function () {
            if (confirm($scope.ConfirmEmptyCacheDirectoryMessage)) {
                $scope.showResultPanel = false;
                $scope.showLoader = false;
                $scope.showDeletionLoader = true;
                hkCacheResource.emptyCacheDirectory().then(function (response) {
                    if (response.data == 'true') {
                        notificationsService.add($scope.emptyCacheDirectorySuccessNotification);
                    }
                    else {
                        notificationsService.add($scope.emptyCacheDirectoryErrorNotification);
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

        // Create FALM Service Page
        $scope.createServicePageSuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_CacheTempManager.CreateFALMServicePageSuccessHeadline").then(function (value) {
            $scope.createServicePageSuccessNotification.headline = value;
        });
        localizationService.localize("FALM_CacheTempManager.CreateFALMServicePageSuccessMessage").then(function (value) {
            $scope.createServicePageSuccessNotification.message = value;
        });

        $scope.createServicePageErrorNotification = {
            'type': 'error',
            'sticky': false
        };
        localizationService.localize("FALM_CacheTempManager.CreateFALMServicePageErrorHeadline").then(function (value) {
            $scope.createServicePageErrorNotification.headline = value;
        });
        localizationService.localize("FALM_CacheTempManager.CreateFALMServicePageErrorMessage").then(function (value) {
            $scope.createServicePageErrorNotification.message = value;
        });

        localizationService.localize("FALM_CacheTempManager.ConfirmCreateFALMServicePageActionMessage").then(function (value) {
            $scope.ConfirmCreateServicePageActionMessage = value;
        });

        $scope.createServicePage = function () {
            if (confirm($scope.ConfirmCreateServicePageActionMessage)) {
                $scope.showSearchPanel = false;
                $scope.showDeletePanel = false;
                $scope.showLoader = true;
                hkCacheResource.createServicePage().then(function (response) {
                    $scope.createServicePageResult = response.data;
                    if ($scope.createServicePageResult === "true") {
                        notificationsService.add($scope.createServicePageSuccessNotification);
                    }
                    else {
                        notificationsService.add($scope.createServicePageErrorNotification);
                    }
                    $scope.showLoader = false;
                    $scope.showSearchPanel = true;
                    $scope.showDeletePanel = false;
                    $route.reload();
                });
            }
        };
    }

    // Register the controller
    angular.module("umbraco").controller("FALMHousekeepingCacheCleanupController", CacheCleanupController);
})();