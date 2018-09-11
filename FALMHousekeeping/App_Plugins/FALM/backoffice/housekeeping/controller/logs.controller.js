'use strict';
(function () {
    // ######################
    // # DB LOGS CONTROLLER #
    // ######################
    function LogsDBManagerController($route, $scope, $routeParams, $filter, filterFilter, appState, treeService, navigationService, userService, notificationsService, localizationService, dialogService, eventsService, hkLogsResource) {
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

        // GET - View logs
        vm.showLoader = false;
        vm.showSearchPanel = false;

        vm.dblogs = [];

        // Table pagination
        vm.dblogs.pagination = {
            pageNumber: 1,
            totalPages: 1
        };

        vm.dblogs.itemsPerPage = 10;

        // Get DB Logs
        fetchData();

        vm.nextPage = function () {
            if (vm.dblogs.pagination.pageNumber < vm.dblogs.pagination.totalPages) {
                vm.dblogs.pagination.pageNumber++;
                fetchData();
            }
        };

        vm.prevPage = function () {
            if (vm.dblogs.pagination.pageNumber > 1) {
                vm.dblogs.pagination.pageNumber--;
                fetchData();
            }
        };

        vm.goToPage = function (pageNumber) {
            vm.dblogs.pagination.pageNumber = pageNumber;
            fetchData();
        };

        // Logs filter
        vm.q = '';

        vm.filterDBLogs = function () {
            vm.dblogs.pagination.pageNumber = "1";
            fetchData();
        };

        // Label CSS Class
        vm.getLabelType = function (LogHeader) {
            switch (LogHeader) {
                case 'Error':
                case 'LoginFailure':
                    return 'label label-danger';
                //break;
                case 'Publish':
                    return 'label label-success';
                //break;
                case 'UnPublish':
                    return 'label label-info';
                //break;
                case 'SendToPublish':
                case 'SendToTranslate':
                    return 'label label-warning';
                //break;
                default:
                    return 'label label-default';
            }
        };

        // Open detail modal
        vm.openDetailsModal = function (logItem, filteredLogs) {
            var dialog = dialogService.open({
                template: '/App_Plugins/FALM/backoffice/housekeeping/view/logs-dbmanager-modal-details.html',
                dialogData: {
                    logItem: logItem,
                    filteredLogs: filteredLogs
                },
                show: true,
                width: 800
            });
        };

        // POST - Delete Showed Logs
        vm.logsSuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_LogsManager.Cleanup.DeleteLogsSuccessHeadline").then(function (value) {
            vm.logsSuccessNotification.headline = value;
        });
        localizationService.localize("FALM_LogsManager.Cleanup.DeleteLogsSuccessMessage").then(function (value) {
            vm.logsSuccessNotification.message = value;
        });

        vm.logsErrorNotification = {
            "type": "error",
            "headline": "",
            "sticky": false
        };
        localizationService.localize("FALM_LogsManager.Cleanup.DeleteLogsErrorHeadline").then(function (value) {
            vm.logsErrorNotification.headline = value;
        });
        localizationService.localize("FALM_LogsManager.Cleanup.DeleteLogsErrorMessage").then(function (value) {
            vm.logsErrorNotification.message = value;
        });
        localizationService.localize("FALM_LogsManager.Cleanup.ConfirmationMessage").then(function (value) {
            vm.confirmDeleteActionMessage = value;
        });
        localizationService.localize("FALM_LogsManager.Cleanup.confirmDeleteLogOlder6MonthsActionMessage").then(function (value) {
            vm.confirmDeleteLogOlder6MonthsActionMessage = value;
        });

        vm.showDeletePanel = false;
        vm.showDeleteLoader = false;

        // Delete filtered Logs via hkLogsResource
        vm.deleteFilteredDBLogs = function (filteredLogs) {
            if (confirm(vm.confirmDeleteActionMessage)) {
                vm.showSearchPanel = false;
                vm.showLoader = true;
                hkLogsResource.deleteFilteredDBLogs(filteredLogs).then(function (response) {
                    if (response.data === "true") {
                        notificationsService.add(vm.logsSuccessNotification);
                    }
                    else {
                        notificationsService.add(vm.logsErrorNotification);
                    }
                });
                vm.showLoader = false;
                vm.showSearchPanel = true;
                $route.reload();
            }
        };

        vm.deleteDBLogsBeforeMonths = function () {
            if (confirm(vm.confirmDeleteLogOlder6MonthsActionMessage)) {
                vm.showSearchPanel = false;
                vm.showLoader = true;
                hkLogsResource.deleteDBLogsBeforeMonths().then(function (response) {
                    if (response.data === "true") {
                        notificationsService.add(vm.logsSuccessNotification);
                    }
                    else {
                        notificationsService.add(vm.logsErrorNotification);
                    }
                    vm.showLoader = false;
                    vm.showSearchPanel = true;
                    $route.reload();
                });
            }
        };

        // Tree Node Highlight
        var activeNode = appState.getTreeState("selectedNode");
        if (activeNode) {
            var activeNodePath = treeService.getPath(activeNode).join();
            navigationService.syncTree({ tree: $routeParams.tree, path: activeNodePath, forceReload: false, activate: true });
        } else {
            navigationService.syncTree({ tree: $routeParams.tree, path: ["-1", "logs", $routeParams.id], forceReload: false, activate: true });
        }

        // DB Logs Functions
        function fetchData() {
            // Get all db logs via hkLogsResource
            hkLogsResource.getDBLogs(vm.q, vm.dblogs.itemsPerPage, vm.dblogs.pagination.pageNumber).then(function (response) {
                vm.showSearchPanel = false;
                vm.showLoader = true;

                vm.dblogs.logsItems = response.data.ListDBLogs;
                vm.dblogs.pagination.totalPages = response.data.TotalPages;
                vm.dblogs.pagination.pageNumber = response.data.CurrentPage;
                vm.dblogs.itemCount = response.data.TotalItems;

                vm.showLoader = false;   // hide loader
                vm.showSearchPanel = true;
            }, function (response) {
                notificationsService.error("Error", "Could not load log data.");
            });

            vm.dblogs.rangeTo = (vm.dblogs.itemsPerPage * (vm.dblogs.pagination.pageNumber - 1)) + vm.dblogs.itemCount;
            vm.dblogs.rangeFrom = (vm.dblogs.rangeTo - vm.dblogs.itemCount) + 1;

            vm.sortType = 'LogDate'; // set the default sort type
            vm.reverse = true;       // set the default sort order
        }
    }

    // #########################
    // # TRACE LOGS CONTROLLER #
    // #########################
    function LogsTLManagerController($route, $scope, $routeParams, $filter, filterFilter, appState, treeService, navigationService, hkLogsResource, userService, notificationsService, localizationService, dialogService, eventsService) {
        // ViewModel
        var vm = this;

        // Set a property on the scope equal to the current route id
        vm.id = $routeParams.id;

        // Reload page function
        vm.reloadRoute = function () {
            $route.reload();
        };

        vm.tracelogs = [];
        vm.tracelogs.allTraceLogs = [];
        vm.tracelogs.logsItems = [];
        vm.tracelogs.itemCount = 0;
        vm.tracelogs.itemsPerPage = 10;
        vm.tracelogs.pagination = {
            pageNumber: 1,
            totalPages: 1
        };


        // Get Trace Logs
        vm.showLoader = true;
        vm.showSearchPanel = false;
        vm.showNoTLLogsFound = false;

        // Get all Trace Logs (paged)
        fetchData();

        vm.nextPage = function () {
            if (vm.tracelogs.pagination.pageNumber < vm.tracelogs.pagination.totalPages) {
                vm.tracelogs.pagination.pageNumber++;
                fetchData();
            }
        };

        vm.prevPage = function () {
            if (vm.tracelogs.pagination.pageNumber > 1) {
                vm.tracelogs.pagination.pageNumber--;
                fetchData();
            }
        };

        vm.goToPage = function (pageNumber) {
            vm.tracelogs.pagination.pageNumber = pageNumber;
            fetchData();
        };

        // Logs filter
        vm.q = '';

        vm.filterTraceLogs = function () {
            vm.tracelogs.pagination.pageNumber = "1";
            fetchData();
        };

        // Table sort
        vm.sortType = '';
        vm.reverse = false;
        vm.sort = function (keyname) {
            vm.sortKey = keyname;       //set the sortKey to the param passed
            vm.reverse = !vm.reverse;   //if true make it false and vice versa
        };

        // Label CSS Class
        vm.getLabelType = function (LogLabel) {
            switch (LogLabel) {
                case 'ERROR':
                    return 'label label-danger';
                //break;
                case 'INFO':
                    return 'label label-info';
                //break;
                case 'WARN':
                    return 'label label-warning';
                //break;
                default:
                    return 'label label-default';
            }
        };

        // Open detail modal
        vm.openDetailsModal = function (logItem, filteredLogs) {
            var dialog = dialogService.open({
                template: '/App_Plugins/FALM/backoffice/housekeeping/view/logs-tlmanager-modal-details.html',
                dialogData: {
                    logItem: logItem,
                    filteredLogs: filteredLogs
                },
                show: true,
                width: 800
            });
        };

        // Tree Node Highlight
        var activeNode = appState.getTreeState("selectedNode");
        if (activeNode) {
            var activeNodePath = treeService.getPath(activeNode).join();
            navigationService.syncTree({ tree: $routeParams.tree, path: activeNodePath, forceReload: false, activate: true });
        } else {
            navigationService.syncTree({ tree: $routeParams.tree, path: ["-1", "logs", $routeParams.id], forceReload: false, activate: true });
        }

        // Trace Logs Functions
        function fetchData() {
            if (vm.q == null || vm.q == '') {
                // Get all trace logs via hkLogsResource
                hkLogsResource.getTraceLogs(vm.id, vm.tracelogs.itemsPerPage, vm.tracelogs.pagination.pageNumber).then(function (response) {
                    vm.showSearchPanel = false;
                    vm.showLoader = true;

                    vm.tracelogs.allTraceLogs = response.data.ListAllTraceLogs;
                    vm.tracelogs.logsItems = response.data.ListTraceLogs;
                    vm.tracelogs.pagination.totalPages = response.data.TotalPages;
                    vm.tracelogs.pagination.pageNumber = response.data.CurrentPage;
                    vm.tracelogs.itemCount = response.data.TotalItems;

                    vm.showLoader = false;
                    vm.showSearchPanel = true;
                }, function (response) {
                    notificationsService.error("Error", "Could not load log data.");
                });
            } else {
                // Get filtered trace logs via hkLogsResource
                hkLogsResource.filterTraceLogs(vm.id, vm.tracelogs.allTraceLogs, vm.tracelogs.allFilteredTraceLogs, vm.q, vm.tracelogs.previousSearch, vm.tracelogs.itemsPerPage, vm.tracelogs.pagination.pageNumber).then(function (response) {
                    vm.showSearchPanel = false;
                    vm.showLoader = true;

                    vm.tracelogs.allFilteredTraceLogs = response.data.ListAllFilteredTraceLogs;
                    vm.tracelogs.logsItems = response.data.ListTraceLogs;
                    vm.tracelogs.previousSearch = response.data.PreviousSearch;
                    vm.tracelogs.pagination.totalPages = response.data.TotalPages;
                    vm.tracelogs.pagination.pageNumber = response.data.CurrentPage;
                    vm.tracelogs.itemCount = response.data.TotalItems;

                    vm.showLoader = false;
                    vm.showSearchPanel = true;
                }, function (response) {
                    notificationsService.error("Error", "Could not load log data.");
                });
            }

            vm.tracelogs.rangeTo = (vm.tracelogs.itemsPerPage * (vm.tracelogs.pagination.pageNumber - 1)) + vm.tracelogs.itemCount;
            vm.tracelogs.rangeFrom = (vm.tracelogs.rangeTo - vm.tracelogs.itemCount) + 1;

            vm.sortType = 'LogDate'; // set the default sort type
            vm.reverse = true;       // set the default sort order
        }

        $scope.$watch('vm.tracelogs.pagination.totalPages', function (term) {
            //alert("Total Pages: " + vm.tracelogs.pagination.totalPages);
        });
    }

    // Register controllers
    angular.module("umbraco").controller("FALMHousekeepingLogsDBManagerController", LogsDBManagerController);
    angular.module("umbraco").controller("FALMHousekeepingLogsTLManagerController", LogsTLManagerController);
})();