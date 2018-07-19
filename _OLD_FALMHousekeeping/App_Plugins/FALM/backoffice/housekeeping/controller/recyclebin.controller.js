'use strict';
(function () {
    // Create Edit controller
    function RecycleBinCleanupController($route, $scope, $filter, $routeParams, appState, treeService, navigationService, hkRecycleBinResource, userService, notificationsService, localizationService, eventsService) {
        // Set a property on the scope equal to the current route id
        $scope.id = $routeParams.id;

        // Reload page function
        $scope.reloadRoute = function () {
            $route.reload();
        };

        $scope.showSearchPanel = true;
        $scope.showLoader = true;
        $scope.showDeletePanel = false;

        // Get items in all Recycle Bins
        hkRecycleBinResource.getAllItemsInRecycleBins().then(function (response) {
            $scope.showSearchPanel = true;
            $scope.showDeletePanel = false;
            $scope.showLoader = true;
            $scope.recycleBins = response.data;
            $scope.contentItems = $scope.recycleBins.ListItemsInRecycleBins[0].Value; // content value
            $scope.mediaItems = $scope.recycleBins.ListItemsInRecycleBins[1].Value; // media value
            $scope.showLoader = false;
        });

        // Empty Content Recycle Bin
        $scope.emptyContentRecycleBinSuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_RecycleBinManager.EmptyContentRecycleBinSuccessHeadline").then(function (value) {
            $scope.emptyContentRecycleBinSuccessNotification.headline = value;
        });
        localizationService.localize("FALM_RecycleBinManager.EmptyContentRecycleBinSuccessMessage").then(function (value) {
            $scope.emptyContentRecycleBinSuccessNotification.message = value;
        });

        $scope.emptyContentRecycleBinErrorNotification = {
            "type": "error",
            "sticky": false
        };
        localizationService.localize("FALM_RecycleBinManager.EmptyContentRecycleBinErrorHeadline").then(function (value) {
            $scope.emptyContentRecycleBinErrorNotification.headline = value;
        });
        localizationService.localize("FALM_RecycleBinManager.EmptyContentRecycleBinErrorMessage").then(function (value) {
            $scope.emptyContentRecycleBinErrorNotification.message = value;
        });

        localizationService.localize("FALM_RecycleBinManager.ConfirmEmptyContentRecycleBinActionMessage").then(function (value) {
            $scope.ConfirmEmptyContentRecycleBinActionMessage = value;
        });

        $scope.emptyContentRecycleBin = function () {
            if (confirm($scope.ConfirmEmptyContentRecycleBinActionMessage)) {
                $scope.showSearchPanel = false;
                $scope.showDeletePanel = true;
                $scope.showLoader = true;
                hkRecycleBinResource.emptyContentRecycleBin().then(function (response) {
                    $scope.recycleBins = response.data;
                    if ($scope.recycleBins.ListItemsInRecycleBins.indexOf("content") !== "-1") {
                        notificationsService.add($scope.emptyContentRecycleBinSuccessNotification);
                    }
                    else {
                        notificationsService.add($scope.emptyContentRecycleBinErrorNotification);
                    }
                    $scope.showLoader = false;
                    $scope.showSearchPanel = true;
                    $scope.showDeletePanel = false;
                    $route.reload();
                });
            }
        };

        // Empty Media Recycle Bin
        $scope.emptyMediaRecycleBinSuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_RecycleBinManager.EmptyMediaRecycleBinSuccessHeadline").then(function (value) {
            $scope.emptyMediaRecycleBinSuccessNotification.headline = value;
        });
        localizationService.localize("FALM_RecycleBinManager.EmptyMediaRecycleBinSuccessMessage").then(function (value) {
            $scope.emptyMediaRecycleBinSuccessNotification.message = value;
        });

        $scope.emptyMediaRecycleBinErrorNotification = {
            "type": "error",
            "sticky": false
        };
        localizationService.localize("FALM_RecycleBinManager.EmptyMediaRecycleBinErrorHeadline").then(function (value) {
            $scope.emptyMediaRecycleBinErrorNotification.headline = value;
        });
        localizationService.localize("FALM_RecycleBinManager.EmptyMediaRecycleBinErrorMessage").then(function (value) {
            $scope.emptyMediaRecycleBinErrorNotification.message = value;
        });

        localizationService.localize("FALM_RecycleBinManager.ConfirmEmptyMediaRecycleBinActionMessage").then(function (value) {
            $scope.ConfirmEmptyMediaRecycleBinActionMessage = value;
        });

        $scope.emptyMediaRecycleBin = function () {
            if (confirm($scope.ConfirmEmptyMediaRecycleBinActionMessage)) {
                $scope.showSearchPanel = false;
                $scope.showDeletePanel = true;
                $scope.showLoader = true;
                hkRecycleBinResource.emptyMediaRecycleBin().then(function (response) {
                    $scope.recycleBins = response.data;
                    if ($scope.recycleBins.ListItemsInRecycleBins.indexOf("media") !== "-1") {
                        notificationsService.add($scope.emptyMediaRecycleBinSuccessNotification);
                    }
                    else {
                        notificationsService.add($scope.emptyMediaRecycleBinErrorNotification);
                    }
                    $scope.showLoader = false;
                    $scope.showSearchPanel = true;
                    $scope.showDeletePanel = false;
                    $route.reload();
                });
            }
        };

        // Empty both Content and Recycle Recycle Bins
        $scope.emptyBothRecycleBinsSuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_RecycleBinManager.EmptyBothRecycleBinsSuccessHeadline").then(function (value) {
            $scope.emptyBothRecycleBinsSuccessNotification.headline = value;
        });
        localizationService.localize("FALM_RecycleBinManager.EmptyBothRecycleBinsSuccessMessage").then(function (value) {
            $scope.emptyBothRecycleBinsSuccessNotification.message = value;
        });

        $scope.emptyBothRecycleBinsErrorNotification = {
            "type": "error",
            "sticky": false
        };
        localizationService.localize("FALM_RecycleBinManager.EmptyBothRecycleBinsErrorHeadline").then(function (value) {
            $scope.emptyBothRecycleBinsErrorNotification.headline = value;
        });
        localizationService.localize("FALM_RecycleBinManager.EmptyBothRecycleBinsErrorMessage").then(function (value) {
            $scope.emptyBothRecycleBinsErrorNotification.message = value;
        });

        localizationService.localize("FALM_RecycleBinManager.ConfirmEmptyBothRecycleBinsActionMessage").then(function (value) {
            $scope.ConfirmEmptyBothRecycleBinsActionMessage = value;
        });

        $scope.emptyBothRecycleBins = function () {
            if (confirm($scope.ConfirmEmptyBothRecycleBinsActionMessage)) {
                $scope.showSearchPanel = false;
                $scope.showDeletePanel = true;
                $scope.showLoader = true;
                hkRecycleBinResource.emptyBothRecycleBins().then(function (response) {
                    $scope.recycleBins = response.data;
                    if ($scope.recycleBins.ListItemsInRecycleBins.indexOf("error") === "-1") {
                        notificationsService.add($scope.emptyBothRecycleBinsSuccessNotification);
                    }
                    else {
                        notificationsService.add($scope.emptyBothRecycleBinsErrorNotification);
                    }
                    $scope.showLoader = false;
                    $scope.showSearchPanel = true;
                    $scope.showDeletePanel = false;
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
            navigationService.syncTree({ tree: $routeParams.tree, path: ["-1", "recyclebin", $routeParams.id], forceReload: false, activate: true });
        }

        // Create FALM Service Page
        $scope.createServicePageSuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_RecycleBinManager.CreateFALMServicePageSuccessHeadline").then(function (value) {
            $scope.createServicePageSuccessNotification.headline = value;
        });
        localizationService.localize("FALM_RecycleBinManager.CreateFALMServicePageSuccessMessage").then(function (value) {
            $scope.createServicePageSuccessNotification.message = value;
        });

        $scope.createServicePageErrorNotification = {
            'type': 'error',
            'sticky': false
        };
        localizationService.localize("FALM_RecycleBinManager.CreateFALMServicePageErrorHeadline").then(function (value) {
            $scope.createServicePageErrorNotification.headline = value;
        });
        localizationService.localize("FALM_RecycleBinManager.CreateFALMServicePageErrorMessage").then(function (value) {
            $scope.createServicePageErrorNotification.message = value;
        });

        localizationService.localize("FALM_RecycleBinManager.ConfirmCreateFALMServicePageActionMessage").then(function (value) {
            $scope.ConfirmCreateServicePageActionMessage = value;
        });

        $scope.createServicePage = function () {
            if (confirm($scope.ConfirmCreateServicePageActionMessage)) {
                $scope.showSearchPanel = false;
                $scope.showDeletePanel = false;
                $scope.showLoader = true;
                hkRecycleBinResource.createServicePage().then(function (response) {
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
    angular.module("umbraco").controller("FALMHousekeepingRecycleBinCleanupController", RecycleBinCleanupController);
})();