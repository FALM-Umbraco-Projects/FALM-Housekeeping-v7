'use strict';
(function () {
    // Create Edit controller
    function VersionsController($route, $scope, $routeParams, $filter, hkVersionsResource, userService, notificationsService, localizationService, dialogService, navigationService, eventsService) {
        // Set a property on the scope equal to the current route id
        $scope.id = $routeParams.id;

        $scope.reloadRoute = function () {
            $route.reload();
        }

        // Select current treenode
        eventsService.on('appState.treeState.changed', function (event, args) {
            if (args.key === 'selectedNode') {

                function buildPath(node, path) {
                    path.push(node.id);
                    if (node.id === '-1') return path.reverse();
                    var parent = node.parent();
                    if (parent === undefined) return path;
                    return buildPath(parent, path);
                }

                event.currentScope.nav.syncTree({
                    tree: $routeParams.tree,
                    path: buildPath(args.value, []),
                    forceReload: false
                });
            }
        });

        localizationService.localize("FALM_VersionsManager.FilterByNodeId").then(function (value) {
            $scope.FilterByNodeId = value;
        });
        localizationService.localize("FALM_VersionsManager.FilterByNodeName").then(function (value) {
            $scope.FilterByNodeName = value;
        });
        localizationService.localize("FALM_VersionsManager.FilterByDateFrom").then(function (value) {
            $scope.FilterByDateFrom = value;
        });
        localizationService.localize("FALM_VersionsManager.FilterByDateTo").then(function (value) {
            $scope.FilterByDateTo = value;
        });

        // GET - VIEW VERSIONS
        $scope.showLoader = false;
        $scope.showSearchPanel = true;

        // Get all versions via hkVersionsResource
        hkVersionsResource.getPublishedNodes().then(function (response) {
            $scope.showSearchPanel = true;
            $scope.showLoader = true;
            $scope.showVersions = false;
            $scope.versions = response.data;
            // $scope.sortType = 'LogDate'; set the default sort type
            // $scope.reverse = true;       set the default sort order
            $scope.showLoader = false;   // hide loader
            $scope.showSearchPanel = false;
            $scope.showVersions = true;
        });

        // Table search
        $scope.search = '';

        // Table sort
        $scope.sortType = '';
        $scope.reverse = false;
        $scope.sort = function (keyname) {
            $scope.sortKey = keyname;           //set the sortKey to the param passed
            $scope.reverse = !$scope.reverse;   //if true make it false and vice versa
        }

        // Table pagination
        $scope.currentPage = 1;
        $scope.pageSize = 10;
        $scope.totalPages = 1;

        // POST - DELETE VIEWED LOGS
        $scope.versionsSuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_VersionsManager.Cleanup.DeleteLogsSuccessHeadline").then(function (value) {
            $scope.versionsSuccessNotification.headline = value;
        });
        localizationService.localize("FALM_VersionsManager.Cleanup.DeleteLogsSuccessMessage").then(function (value) {
            $scope.versionsSuccessNotification.message = value;
        });

        $scope.versionsErrorNotification = {
            "type": "error",
            "headline": "",
            "sticky": false
        };
        localizationService.localize("FALM_VersionsManager.Cleanup.DeleteLogsErrorHeadline").then(function (value) {
            $scope.versionsErrorNotification.headline = value;
        });
        localizationService.localize("FALM_VersionsManager.Cleanup.DeleteLogsErrorMessage").then(function (value) {
            $scope.versionsErrorNotification.message = value;
        });

        localizationService.localize("FALM_VersionsManager.Cleanup.ConfirmationMessage").then(function (value) {
            $scope.confirmDeleteActionMessage = value;
        });

        // Delete filtered Logs via hkLogsResource
        $scope.deleteVersionsByCount = function () {
            if (confirm($scope.confirmDeleteActionMessage)) {
                $scope.showSearchPanel = true;
                $scope.showLoader = true;
                $scope.showVersions = false;
                hkVersionsResource.deleteVersionsByCount(0).then(function (response) {
                    if (response.data = true) {
                        notificationsService.add($scope.versionsSuccessNotification);
                        $route.reload();
                    }
                    else {
                        notificationsService.add($scope.versionsErrorNotification);
                        $scope.showDeletePanel = false;
                    }
                });
                $scope.showSearchPanel = false;
                $scope.showLoader = false;
                $scope.showVersions = true;
            }
        };

        // Open details modal
        $scope.openDetailsModal = function (versionItem) {
            var dialog = dialogService.open({
                template: '/App_Plugins/FALM/backoffice/housekeeping/view/versions-manager-modal-details.html',
                dialogData: {
                    currentPublishedVersionItem: versionItem
                },
                show: true,
                width: 800
            });
        }

        // Open cleanup by count modal
        $scope.openCleanupByCountModal = function () {
            var cbcDialog = dialogService.open({
                template: '/App_Plugins/FALM/backoffice/housekeeping/view/versions-manager-modal-cleanupbycount.html',
                show: true,
                width: 800
            });
        }
    };

    // Register the controller
    angular.module("umbraco").controller("FALMHousekeepingVersionsController", VersionsController);
})();