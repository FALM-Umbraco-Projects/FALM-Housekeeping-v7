'use strict';

// Create Version Edit Controller for Versions Model (Details modal)
function VersionsManagerDetailsController($route, $scope, hkVersionsResource, dialogService, notificationsService, localizationService) {
    $scope.dialogData.showCleanupForm = true;
    $scope.dialogData.showLoader = false;
    $scope.dialogData.showCleanupSummary = false;
    $scope.dialogData.cleanupSummary;
    $scope.histCurrentPage = 1;
    $scope.histPageSize = 10;

    // Get all versions via hkVersionsResource
    $scope.nodeId = $scope.dialogData.currentPublishedVersionItem.NodeId;
    hkVersionsResource.getVersionsByNodeId($scope.nodeId).then(function (response) {
        $scope.histVersions = response.data;
    });

    // POST - DELETE CURRENT NODE VERSIONS
    localizationService.localize("FALM_VersionsManager.CleanupById.Dialog-ConfirmationMessage").then(function (value) {
        $scope.confirmDeleteActionMessage = value;
    });

    // Delete filtered Logs via hkLogsResource
    $scope.deleteVersionsByNodeId = function () {
        if (confirm($scope.confirmDeleteActionMessage)) {
            $scope.dialogData.showLoader = true;
            $scope.dialogData.showCleanupForm = false;
            hkVersionsResource.deleteVersionsByNodeId($scope.nodeId, 0).then(function (response) {
                $scope.dialogData.cleanupSummary = response.data;
            });
            $scope.dialogData.showLoader = false;
            $scope.dialogData.showCleanupSummary = true;
        }
    };

    $scope.closeDialog = function () {
        $scope.dismiss();
    };
}

function VersionsManagerCleanupByCountController($route, $scope, hkVersionsResource, dialogService, notificationsService, localizationService) {
    $scope.dialogData.showCleanupForm = true;
    $scope.dialogData.showLoader = false;
    $scope.dialogData.showCleanupSummary = false;
    $scope.dialogData.cleanupSummary;
    $scope.dialogData.versionsToKeep = 0;

    // Delete filtered Logs via hkLogsResource
    $scope.deleteVersionsByCount = function () {
        if (confirm($scope.dialogData.confirmDeleteActionMessage)) {
            $scope.dialogData.showLoader = true;
            $scope.dialogData.showCleanupForm = false;
            hkVersionsResource.deleteVersionsByCount($scope.dialogData.versionsToKeep).then(function (response) {
                $scope.dialogData.cleanupSummary = response.data;
            });
            $scope.dialogData.showLoader = false;
            $scope.dialogData.showCleanupSummary = true;
        }
    };

    localizationService.localize("FALM_VersionsManager.CleanupByCount.Dialog-ConfirmationMessage").then(function (value) {
        $scope.dialogData.confirmDeleteActionMessage = value;
    });

    $scope.closeDialog = function () {
        $route.reload();
        $scope.dismiss();
    };
}

// Register controllers
angular.module("umbraco").controller("FALMHousekeepingVersionsManagerDetailsController", VersionsManagerDetailsController);
angular.module("umbraco").controller("FALMHousekeepingVersionsManagerCleanupByCountController", VersionsManagerCleanupByCountController);