'use strict';
(function () {
    // ###############################
    // # VERSIONS MANAGER CONTROLLER #
    // ###############################
    function VersionsController($route, $scope, $routeParams, $filter, appState, treeService, navigationService, hkVersionsResource, userService, notificationsService, localizationService, dialogService, eventsService) {
        // ViewModel
        var vm = this;

        // Set a property on the scope equal to the current route id
        vm.id = $routeParams.id;

        // Reload page function
        vm.reloadRoute = function () {
            $route.reload();
        };

        // Select current treenode
        vm.treeDebugNotification = {
            'type': 'success',
            'sticky': false
        };
        vm.treeDebugNotification.headline = "";
        vm.treeDebugNotification.message = $routeParams.tree;

        /*localizationService.localize("FALM_VersionsManager.FilterByNodeId").then(function (value) {
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
        });*/

        // VIEW VERSIONS
        vm.showLoader = false;
        vm.showSearchPanel = false;

        vm.versions = [];

        // Table pagination
        vm.versions.pagination = {
            pageNumber: 1,
            totalPages: 1
        };

        vm.versions.itemsPerPage = 10;

        // Get Versions
        fetchData();

        vm.nextPage = function () {
            if (vm.versions.pagination.pageNumber < vm.versions.pagination.totalPages) {
                vm.versions.pagination.pageNumber++;
                fetchData();
            }
        };

        vm.prevPage = function () {
            if (vm.versions.pagination.pageNumber > 1) {
                vm.versions.pagination.pageNumber--;
                fetchData();
            }
        };

        vm.goToPage = function (pageNumber) {
            vm.versions.pagination.pageNumber = pageNumber;
            fetchData();
        };

        // Versions filter
        vm.q = '';

        vm.filterVersions = function () {
            vm.versions.pagination.pageNumber = "1";
            fetchData();
        };

        // POST - DELETE VIEWED LOGS
        vm.versionsSuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_VersionsManager.Cleanup.DeleteLogsSuccessHeadline").then(function (value) {
            vm.versionsSuccessNotification.headline = value;
        });
        localizationService.localize("FALM_VersionsManager.Cleanup.DeleteLogsSuccessMessage").then(function (value) {
            vm.versionsSuccessNotification.message = value;
        });

        vm.versionsErrorNotification = {
            "type": "error",
            "headline": "",
            "sticky": false
        };
        localizationService.localize("FALM_VersionsManager.Cleanup.DeleteLogsErrorHeadline").then(function (value) {
            vm.versionsErrorNotification.headline = value;
        });
        localizationService.localize("FALM_VersionsManager.Cleanup.DeleteLogsErrorMessage").then(function (value) {
            vm.versionsErrorNotification.message = value;
        });

        localizationService.localize("FALM_VersionsManager.Cleanup.ConfirmationMessage").then(function (value) {
            vm.confirmDeleteActionMessage = value;
        });

        // Delete filtered Logs via hkLogsResource
        vm.deleteVersionsByCount = function () {
            if (confirm(vm.confirmDeleteActionMessage)) {
                vm.showLoader = true;
                hkVersionsResource.deleteVersionsByCount(0).then(function (response) {
                    if (response.data != "null") {
                        notificationsService.add(vm.versionsSuccessNotification);
                    }
                    else {
                        notificationsService.add(vm.versionsErrorNotification);
                        vm.showDeletePanel = false;
                    }
                });
                vm.showLoader = false;
                vm.reload();
            }
        };

        // Open details modal
        vm.openDetailsModal = function (versionItem) {
            var dialog = dialogService.open({
                template: '/App_Plugins/FALM/backoffice/housekeeping/view/versions-manager-modal-details.html',
                dialogData: {
                    currentPublishedVersionItem: versionItem
                },
                show: true,
                width: 800
            });
        };

        // Open cleanup by count modal
        vm.openCleanupByCountModal = function () {
            var cbcDialog = dialogService.open({
                template: '/App_Plugins/FALM/backoffice/housekeeping/view/versions-manager-modal-cleanupbycount.html',
                show: true,
                width: 800
            });
        };

        // TREE NODE HIGHLIGHT
        var activeNode = appState.getTreeState("selectedNode");
        if (activeNode) {
            var activeNodePath = treeService.getPath(activeNode).join();
            navigationService.syncTree({ tree: $routeParams.tree, path: activeNodePath, forceReload: false, activate: true });
        } else {
            navigationService.syncTree({ tree: $routeParams.tree, path: ["-1", "versions", $routeParams.id], forceReload: false, activate: true });
        }

        // Versions Functions
        function fetchData() {
            // Get all published versions via hkVersionsResource
            hkVersionsResource.getPublishedNodes(vm.q, vm.versions.itemsPerPage, vm.versions.pagination.pageNumber).then(function (response) {
                vm.showSearchPanel = false;
                vm.showLoader = true;

                vm.versions.versionsItems = response.data.ListCurrentPublishedVersions;
                vm.versions.pagination.totalPages = response.data.TotalPages;
                vm.versions.pagination.pageNumber = response.data.CurrentPage;
                vm.versions.itemCount = response.data.TotalItems;

                vm.showLoader = false;   // hide loader
                vm.showSearchPanel = true;
            }, function (response) {
                notificationsService.error("Error", "Could not load log data.");
            });

            vm.versions.rangeTo = (vm.versions.itemsPerPage * (vm.versions.pagination.pageNumber - 1)) + vm.versions.itemCount;
            vm.versions.rangeFrom = (vm.versions.rangeTo - vm.versions.itemCount) + 1;
        }
    }

    // Register the controller
    angular.module("umbraco").controller("FALMHousekeepingVersionsController", VersionsController);
})();