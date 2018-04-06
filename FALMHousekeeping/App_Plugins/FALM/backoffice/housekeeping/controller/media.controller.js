'use strict';
(function () {
    // Create Edit controller
    function MediaCleanupController($route, $scope, $routeParams, appState, treeService, navigationService, hkMediaResource, userService, notificationsService, localizationService, eventsService) {
        // Set a property on the scope equal to the current route id
        $scope.id = $routeParams.id;

        $scope.reloadRoute = function () {
            $route.reload();
        };

        $scope.currentUserLanguage = "en-GB";

        userService.getCurrentUser().then(function (user) {
            $scope.currentUserLanguage = user.locale;
        });

        // GET - VIEW MEDIA ORPHANS
        $scope.showSearchPanel = true;
        $scope.showLoader = true;
        $scope.showDeletePanel = false;

        localizationService.localize("FALM_MediaManager.Cleanup.NoMediaOrphansMessage").then(function (value) {
            $scope.NoMediaOrphansMessage = value;
        });
        localizationService.localize("FALM_MediaManager.Cleanup.ConfirmationMessage").then(function (value) {
            $scope.confirmDeleteActionMessage = value;
        });

        // Get all media orphans via hkMediaResource
        hkMediaResource.getMediaToDelete($scope.currentUserLanguage).then(function (response) {
            $scope.showSearchPanel = true;
            $scope.showLoader = true;
            $scope.showDeletePanel = false;
            $scope.media = response.data;
            $scope.mediaToDelete = 0;
            $scope.showLoader = false;
        });

        // Post media orphans to delete via hkUsersResource
        $scope.deleteMediaOrphans = function (mediaOrphansToDelete) {
            if (confirm($scope.confirmDeleteActionMessage)) {
                $scope.showSearchPanel = false;
                $scope.showDeletePanel = true;
                $scope.showLoader = true;
                hkMediaResource.postDeleteMediaOrphans(mediaOrphansToDelete).then(function (response) {
                    $scope.media = response.data;
                });
                $scope.showLoader = false;
            }
        };

        // TREE NODE HIGHLIGHT
        var activeNode = appState.getTreeState("selectedNode");
        if (activeNode) {
            var activeNodePath = treeService.getPath(activeNode).join();
            navigationService.syncTree({ tree: $routeParams.tree, path: activeNodePath, forceReload: false, activate: true });
        } else {
            navigationService.syncTree({ tree: $routeParams.tree, path: ["-1", "media", $routeParams.id], forceReload: false, activate: true });
        }
    }

    // Register the controller
    angular.module("umbraco").controller("FALMHousekeepingMediaCleanupController", MediaCleanupController);
})();