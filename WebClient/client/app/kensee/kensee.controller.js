'use strict';

angular.module('kenseeApp')
  .controller('KenseeCtrl', function ($scope, $http, uiGridConstants, uiGridExporterConstants, $window, $document, SERVER_URL, $timeout, $rootScope) {
    $(document).ready(function () {
      /**********************************************/
      /****** Dashboard Map Functions  **************/
      /**********************************************/

      function initialize() {
        var latlng = new google.maps.LatLng(51.507351, -0.127758);
        var myOptions = {
          zoom: 11,
          center: latlng,
          mapTypeId: google.maps.MapTypeId.ROADMAP
        };
        var map = new google.maps.Map(document.getElementById("map"),
          myOptions);
      }
      //google.maps.event.addDomListener(window, "load", initialize);
      initialize();

    /*  var polygon;
      var map = new GMaps({
        div: '#map',
        zoom: 11,
        lat: 51.507351,
        lng: -0.127758
      });
*/
      /****** Add marker on address  **************/
     /* $('#Address').blur(function () {
        GMaps.geocode({
          address: $('#Address').val(),
          callback: function (results, status) {
            if (status == 'OK') {
              var latlng = results[0].geometry.location;
              map.setCenter(latlng.lat(), latlng.lng());
              map.addMarker({
                lat: latlng.lat(),
                lng: latlng.lng()
              });
            }
          }
        });
      });
*/
      var polygon;
      $('#ShowRang').change(function () {
        if ($(this).is(':checked')) {
          /****** Add polygon **************/
          var pathSoho = [
            [51.51533, -0.14161],
            [51.51965, -0.11233],
            [51.51285, -0.08751],
            [51.49451, -0.09906],
            [51.49241, -0.13327],
            [51.50195, -0.14534]
          ];

          polygon = map.drawPolygon({
            paths: pathSoho, // pre-defined polygon shape
            strokeColor: '#1c1a5b',
            strokeOpacity: 1,
            strokeWeight: 3,
            fillColor: '#544ff2',
            fillOpacity: 0.6
          });


        }
        else {
          polygon.setMap(null);
        }
      });

      var addReport = $('.btn.btn-minier.btn-danger');
      var tabs = $('.tabs li');
      var reportsLink = $('.goReports');
      var contents = $('.page-content');
      var logout = $('.logout');
      var reports = {
        rent: [],
        investment: [],
        construction: []
      };

      $(addReport).on('click', function(){
        var parentContainer = popTo(event.target, 'collapse in');
        //var chart = parentContainer.find('canvas')[0];

        if(reports[parentContainer.type].indexOf(parentContainer.title) == -1) {
          reports[parentContainer.type].push(parentContainer.title);
          var clone = $(parentContainer.element.cloneNode(true));
          $('#reports-' + parentContainer.type).append(clone[0]);

          if(parentContainer.canvas) {

            var ctx = clone.find('canvas')[0].getContext('2d');
            var newImage = new Image;
            newImage.src = parentContainer.canvas;
            ctx.drawImage(newImage, 0, 0);
          }

          clone.append('<button class="btn btn-primary btn-success add-note"><i class="fa fa-pencil-square-o"></i> Add a note</button>' +
            '<textarea rows="3" class="report-note form-control" placeholder="Leave your note here..."></textarea><i class="fa fa-close close-note"></i>');
          var reportNote = clone.find('.report-note');
          var addNote = clone.find('.add-note');
          addNote.on('click', function(){
            var _this = $(this);
            _this.slideUp(500, function(){
              reportNote.slideDown(500, function(){
                closeNote.fadeIn();
              });

            });
          });

          var closeNote = clone.find('.close-note');
          closeNote.on('click', function(){
            closeNote.fadeOut();
            reportNote.slideUp(500, function(){
              addNote.slideDown(500);
            });
          });

          clone.find('.btn.btn-minier.btn-danger').html('<i class="fa fa-trash-o"></i> Remove from report').on('click', function(){
            var type = clone[0].parentElement.getAttribute('id').slice(8);
            console.log(reports);
            reports[type].splice(reports[type].indexOf(clone.find('.widget-title').text()),1);
            console.log(reports);
            clone.remove();

          });
        }

        reportsLink.removeClass('disabledNavbarLink');
      });

      reportsLink.on('click', function(){
        $scope.showReports = true;
      });

      function popTo(element, targetClassName){
        var result = {};
        var parentElement = element.parentElement;
        while(parentElement.className !== targetClassName && parentElement){
          if(parentElement.className == 'widget-box widget-color-green2 light-border'){
            result.element = parentElement.parentElement;
            result.canvas = $(result.element).find('canvas').length > 0 ? $(result.element).find('canvas')[0].toDataURL() : null;
            result.title = $(result.element).find('.widget-title').text();
          }
          parentElement = parentElement.parentElement;
          result.type = $(parentElement).prev().text().trim().toLowerCase();
        }
        return result;
      }

    });

    $rootScope.showReports = false;

    $rootScope.showPage = false;
    $scope.showRange = false;
    $scope.showReports = false;
    $scope.options = [];
    $scope.types = [];
    $scope.selected = {};
    $scope.selectedC = {};
    $scope.selectedType = {};
    $scope.parameters = {
      Area: '',
      AreaC: '',
      Sub: '',
      SubC: '',
      District: '',
      DistrictC: '',
      PropertyType: ''
    };

    $scope.runKensee = function(){
      console.log($scope.parameters);
      $rootScope.showPage = true;
      $rootScope.showReports = false;

      var Area = $scope.parameters.Area;
      var Sub = $scope.parameters.Sub;
      var District = $scope.parameters.District;
      var AreaC = $scope.parameters.AreaC;
      var SubC = $scope.parameters.SubC;
      var DistrictC = $scope.parameters.DistrictC;
      var PropertyType = $scope.parameters.PropertyType;
      $('#lblArea').html(Area + " - " + Sub);
      $('#lblAreaC').html(AreaC + " - " + SubC);
      $('#lblType').html(PropertyType);

      if (Area != '') {
        $('#AreaOverview').removeClass('hide');
        $('#AreaOverviewLbl').html(Area + " - " + Sub);
        $('#AreaStats').removeClass('hide');
        $('#AreaStatsLbl').html(Area + " - " + Sub);
        $.ajax({
          type: "GET",
          url: "assets/xml/texts.xml",
          dataType: "xml",
          success: function (xml) {
            $(xml).find('area').each(function () {
              if ($(this).attr('name') == Area && $(this).attr('sub') == Sub) {
                $('#AreaOverviewTxt').html($(this).children('overview').text());
                $('#AreaStatsTxt').html($(this).children('stats').text());
              }
            });

          }
        });
      }
      if (AreaC != '') {
        $('#AreaCOverview').removeClass('hide');
        $('#AreaCOverviewLbl').html(AreaC + " - " + SubC);
        $('#AreaCStats').removeClass('hide');
        $('#AreaCStatsLbl').html(AreaC + " - " + SubC);
        $.ajax({
          type: "GET",
          url: "assets/xml/texts.xml",
          dataType: "xml",
          success: function (xml) {
            $(xml).find('area').each(function () {
              if ($(this).attr('name') == Area && $(this).attr('sub') == Sub) {
                $('#AreaCOverviewTxt').html($(this).children('overview').text());
                $('#AreaCStatsTxt').html($(this).children('stats').text());
              }
            });

          }
        });
      }


      /**********************************************/
      /****** Charts  *******************************/
      /**********************************************/

      /****** Rent Takeup  ***************************/
        //var dataArea = "";
        //var dataAreaC = "";
        //$.ajax({
        //    type: "GET",
        //    url: "assets/xml/rent.xml",
        //    dataType: "xml",
        //    success: function (xml) {
        //        $(xml).find('item').each(function () {
        //            if ($(this).attr('area') == Area) {
        //                dataArea += $(this).attr('value') + ',';
        //            }
        //            if ($(this).attr('area') == AreaC) {
        //                dataAreaC += $(this).attr('value') + ',';
        //            }
        //        });

        //    }
        //});
        //console.log(dataArea);
        //console.log(dataAreaC);
        $timeout(function() {


          var data;
          data = {};
          data["City"] = [0.79, 0.66, 0.98, 0.99, 0.8, 1.21, 0.84, 1.21, 1.05, 1.94, 2.09, 1.95, 1.15, 1.6, 2.35, 1.87, 1.68, 1.62, 1.91, 1.54, 1.39];
          data["West-End"] = [0.67, 0.85, 0.65, 1.1, 0.81, 0.55, 0.62, 0.64, 1.4, 0.6, 0.78, 0.68, 0.75, 0.9, 0.98, 0.64, 0.94, 0.91, 0.81, 1.02, 0.89];
          data["Docklands"] = [0, 0.06, 0.31, 0.09, 0.2, 0.1, 0.19, 0.08, 0.06, 0.16, 0.29, 0.09, 0.47, 0.17, 0.19, 0.36, 0.11, 1.26, 0.19, 0.3, 0.39];
          var dataset1 = "";
          var dataset2 = "";
          if (Area == 'City' || Area == 'West-End' || Area == 'Docklands') {
            dataset1 = {
              label: Area,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(179,181,198,0.2)",
              borderColor: "rgba(179,181,198,1)",
              pointBackgroundColor: "rgba(179,181,198,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(179,181,198,1)",
              data: data[Area]
            };
          }
          if (AreaC == 'City' || AreaC == 'West-End' || AreaC == 'Docklands') {
            dataset2 = {
              label: AreaC,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(255,99,132,0.2)",
              borderColor: "rgba(255,99,132,1)",
              pointBackgroundColor: "rgba(255,99,132,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(255,99,132,1)",
              data: data[AreaC]
            };
          }

          if (dataset1 != "" || dataset2 != "") {
            var datasets = new Array();
            if (dataset1 != "")
              datasets.push(dataset1);
            if (dataset2 != "")
              datasets.push(dataset2);

            var options = {
              scales: {
                yAxes: [{
                  scaleLabel: {
                    display: true,
                    labelString: 'Take up (million sq ft)'
                  }
                }]
              },
              responsive: true,
              "chartArea": {
                "width": '100%',
                "height": '100%'
              }
            };

            var RentTakeup = $("#rent-takeup-chart");
            var dataRentTakeup = {
              labels: ["2011-Q1", "2011-Q2", "2011-Q3", "2011-Q4", "2012-Q1", "2012-Q2", "2012-Q3", "2012-Q4", "2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
              datasets: datasets
            };
            var myLineChart = new Chart(RentTakeup, {
              type: 'line',
              data: dataRentTakeup,
              options: options
            });
          }
          else {
            $('#RentTakeup').hide();
          }
          /*

           /****** Rent Availability  ***************************/
          data = new Array();
          data["City"] = [7.4, 7.07, 7.09, 7.13, 7.38, 7.33, 7.59, 7.31, 6.54, 6.34, 5.79, 5.18, 4.73, 4.49, 5.15, 5.5, 5.95];
          data["West-End"] = [4.4, 3.8, 3.7, 3.5, 3.1, 3.4, 3.8, 4, 4.1, 4.5, 4.3, 4.4, 4.5, 4.3, 4.4, 4.4, 4.3];
          data["Docklands"] = [1.33, 1.24, 1.37, 1.5, 1.71, 1.88, 1.76, 1.44, 1.24, 1.33, 1.32, 1.33, 1.54, 1.32, 1.14, 1.53, 1.15];
          data["Southbank"] = [1.51, 1.87, 1.82, 1.93, 2.39, 2.2, 1.76, 1.57, 1.58, 1.24, 1.18, 0.93, 0.98, 1.17, 1.31, 1.53, 1.5];
          data["Midtown"] = [1.38, 1.26, 1.35, 1.47, 1.58, 1.79, 1.68, 1.39, 1.1, 1.2, 1.21, 1.14, 1.36, 1.14, 0.97, 1.29, 0.97];
          var dataset1 = "";
          var dataset2 = "";
          if (Area == 'City' || Area == 'West-End' || Area == 'Docklands' || Area == 'Southbank' || Area == 'Midtown') {
            dataset1 = {
              label: Area,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(179,181,198,0.2)",
              borderColor: "rgba(179,181,198,1)",
              pointBackgroundColor: "rgba(179,181,198,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(179,181,198,1)",
              data: data[Area]
            };
          }
          if (AreaC == 'City' || AreaC == 'West-End' || AreaC == 'Docklands' || AreaC == 'Southbank' || AreaC == 'Midtown') {
            dataset2 = {
              label: AreaC,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(255,99,132,0.2)",
              borderColor: "rgba(255,99,132,1)",
              pointBackgroundColor: "rgba(255,99,132,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(255,99,132,1)",
              data: data[AreaC]
            };
          }

          if (dataset1 != "" || dataset2 != "") {
            var datasets = new Array();
            if (dataset1 != "")
              datasets.push(dataset1);
            if (dataset2 != "")
              datasets.push(dataset2);

            options = {
              scales: {
                yAxes: [{
                  scaleLabel: {
                    display: true,
                    labelString: 'Availbility %'
                  }
                }]
              }
            };

            var RentAvailability = $("#rent-availability-chart");
            var dataRentAvailability = {
              labels: ["2012-Q1", "2012-Q2", "2012-Q3", "2012-Q4", "2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
              datasets: datasets
            };
            var myLineChart = new Chart(RentAvailability, {
              type: 'line',
              data: dataRentAvailability,
              options: options
            });
          }
          else {
            $('#RentAvailability').hide();
          }

          /****** Rent Vacancy  ***************************/
          data = new Array();
          data["City"] = [8, 7.93, 6.83, 6.65, 5.48, 4.82, 4.02, 3.98, 4.42, 4.97, 5.41, 5.59, 7.93, 8.95, 9.36, 9.5, 7.42, 7.67, 7.42, 6.87, 7.31, 6.76, 7.45, 6.69, 6.47, 7.67, 7.38, 7.2, 6.94, 7.16, 6.32, 6.06, 7.09, 6.58, 6.06, 6.32, 5.7, 4.82, 4.02, 3.72, 3.14];
          data["West-End"] = [4.67, 4.38, 4.07, 3.76, 3.32, 3.17, 3.36, 3.32, 3.17, 3.65, 4.44, 5.88, 6.5, 7.27, 7.53, 7.07, 6.69, 6.21, 5.3, 4.42, 4.13, 3.83, 3.54, 3.28, 3.8, 3.83, 3.38, 3.87, 4.31, 4.05, 3.5, 3.32, 2.66, 2.33, 2.26, 2.84, 2.44, 2.3, 2.22, 2.3, 2.48];
          data["Docklands"] = [9.32, 9.36, 8.59, 7.89, 7.53, 5.3, 3.61, 3.87, 3.03, 2.63, 2.95, 2.44, 3.17, 8.41, 13.05, 12.39, 10.6, 7.86, 7.27, 6.83, 7.35, 7.42, 6.98, 5.88, 5.59, 6.54, 6.83, 6.61, 6.32, 6.03, 6.58, 7.16, 6.58, 5.59, 5.48, 6.58, 6.94, 6.14, 5.33, 4.82, 4.45];
          data["Southbank"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2.4, 2.7, 2.2, 3.1, 2.6, 2.5];
          data["Midtown"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2.3, 1.8, 2.6, 2.3, 2.1, 2.9];
          data["Central London"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2.6, 2.3, 2.6, 2.7, 3, 3.3];
          var dataset1 = "";
          var dataset2 = "";
          if (Area == 'City' || Area == 'West-End' || Area == 'Docklands' || Area == 'Southbank' || Area == 'Midtown' || Area == 'Central London') {
            dataset1 = {
              label: Area,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(179,181,198,0.2)",
              borderColor: "rgba(179,181,198,1)",
              pointBackgroundColor: "rgba(179,181,198,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(179,181,198,1)",
              data: data[Area]
            };
          }
          if (AreaC == 'City' || AreaC == 'West-End' || AreaC == 'Docklands' || AreaC == 'Southbank' || AreaC == 'Midtown' || AreaC == 'Central London') {
            dataset2 = {
              label: AreaC,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(255,99,132,0.2)",
              borderColor: "rgba(255,99,132,1)",
              pointBackgroundColor: "rgba(255,99,132,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(255,99,132,1)",
              data: data[AreaC]
            };
          }

          if (dataset1 != "" || dataset2 != "") {
            var datasets = new Array();
            if (dataset1 != "")
              datasets.push(dataset1);
            if (dataset2 != "")
              datasets.push(dataset2);

            options = {
              scales: {
                yAxes: [{
                  scaleLabel: {
                    display: true,
                    labelString: 'Vacancy Rate in %'
                  }
                }]
              }
            };

            var RentVacancy = $("#rent-vacancy-chart");
            var dataRentVacancy = {
              labels: ["2006-Q1", "2006-Q2", "2006-Q3", "2006-Q4", "2007-Q1", "2007-Q2", "2007-Q3", "2007-Q4", "2008-Q1", "2008-Q2", "2008-Q3", "2008-Q4", "2009-Q1", "2009-Q2", "2009-Q3", "2009-Q4", "2010-Q1", "2010-Q2", "2010-Q3", "2010-Q4", "2011-Q1", "2011-Q2", "2011-Q3", "2011-Q4", "2012-Q1", "2012-Q2", "2012-Q3", "2012-Q4", "2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
              datasets: datasets
            };
            var myLineChart = new Chart(RentVacancy, {
              type: 'line',
              data: dataRentVacancy,
              options: options
            });
          }
          else {
            $('#RentVacancy').hide();
          }

          /****** Rent Prime  ***************************/
          data = new Array();
          data["City"] = [56.04, 56.3, 49.65, 50.75, 51.75, 51.95, 53.85, 55.6, 59.05, 59.95, 61.35, 62.85, 64.6];
          data["City - City Core"] = [0, 0, 57.5, 58.5, 58.5, 58.5, 60, 60, 63.5, 63.5, 65, 67.5, 70];
          data["City - Chancery Lane/Midtown"] = [0, 0, 57.5, 57.5, 51.75, 57.5, 60, 60, 60, 62.5, 62.5, 63.5, 65];
          data["City - City Eastern Fringe"] = [0, 0, 40, 40, 42.5, 42.5, 45, 47.5, 50, 50, 55, 55, 0];
          data["City - City Northern Fringe"] = [0, 0, 47.5, 50, 52.5, 52.5, 55, 58.5, 63.5, 63.5, 63.5, 63.5, 63.5];
          data["City - Insurance Sector"] = [0, 0, 55, 55, 57.5, 57.5, 57.5, 57.5, 60, 60, 60, 62.5, 0];
          data["City - London Bridge/More London"] = [0, 0, 46.5, 47.5, 47.5, 47.5, 50, 52.5, 57.5, 57.5, 57.5, 59.5, 62.5];
          data["City - Waterloo/Bankside"] = [0, 0, 47.5, 48, 48, 50, 52.5, 52.5, 57.5, 57.5, 57.5, 59.5, 62.5];
          data["City - West City"] = [0, 0, 57.5, 58.5, 58.5, 58.5, 58.5, 60, 63.5, 65, 65, 67.5, 70];
          data["City - Shoreditch"] = [0, 0, 37.5, 42.5, 45, 45, 47.5, 52.5, 55, 60, 62.5, 65, 65];
          data["City - Clerkenwell"] = [0, 0, 50, 50, 50, 50, 52.5, 55, 60, 60, 65, 65, 65];

          data["West-End"] = [97.19, 96.69, 69.95, 66.69, 68.42, 70.87, 74.92, 76.15, 79.6, 81.47, 83.71, 84.73, 84.92];
          data["West-End - Battersea"] = [0, 0, 27.5, 75, 27.5, 29.5, 0, 0, 0, 0, 0, 0, 0];
          data["West-End - Belgravia/Knightsbridge"] = [0, 0, 75, 55, 77.5, 80, 82.5, 85, 90, 95, 100, 100, 100];
          data["West-End - Bloomsbury (WC1)"] = [0, 0, 55, 35, 55, 55, 58, 62.5, 67.5, 70, 72.5, 72.5, 72.5];
          data["West-End - Camden"] = [0, 0, 35, 70, 35, 42.5, 42.5, 45, 50, 52.5, 55, 55, 55];
          data["West-End - Chelsea"] = [0, 0, 70, 67.5, 70, 72.5, 80, 85, 90, 95, 100, 100, 100];
          data["West-End - Covent Garden"] = [0, 0, 67.5, 62.5, 70, 77.5, 80, 80, 80, 83, 85, 85, 85];
          data["West-End - Euston"] = [0, 0, 62.5, 72.5, 72.5, 77.5, 68.5, 68.5, 72.5, 72.5, 75, 75, 75];
          data["West-End - Fitzrovia"] = [0, 0, 72.5, 35, 72.5, 77.5, 80, 80, 80, 82.5, 85, 85, 85];
          data["West-End - Fulham"] = [0, 0, 35, 38, 35, 37.5, 37.5, 80, 40, 40, 42.5, 45, 45];
          data["West-End - Hammersmith"] = [0, 0, 36.5, 50, 48.5, 48.5, 48.5, 48.5, 50, 50, 51, 55, 56];
          data["West-End - Kensington"] = [0, 0, 47.5, 60, 60, 52.5, 55, 55, 57.5, 60, 62.5, 65, 65];
          data["West-End - King's Cross"] = [0, 0, 57.5, 110, 60, 68.5, 68.5, 70, 75, 77.5, 80, 82.5, 82.5];
          data["West-End - Mayfair"] = [0, 0, 107.5, 130, 115, 115, 115, 117.5, 117.5, 120, 120, 120, 120];
          data["West-End - Mayfair/St James's"] = [0, 0, 120, 120, 92.5, 130, 130, 130, 130, 150, 150, 152, 150];
          data["West-End - North of Oxford St/Marylebone"] = [0, 0, 92.5, 57.5, 92.5, 92.5, 92.5, 92.5, 95, 95, 97.5, 97.5, 97.5];
          data["West-End - Paddington"] = [0, 0, 57.5, 85, 57.5, 57.5, 60, 60, 62.5, 62.5, 95, 67.5, 70];
          data["West-End - Soho"] = [0, 0, 85, 110, 85, 85, 85, 85, 87.5, 90, 95, 95, 95];
          data["West-End - St James's"] = [0, 0, 107.5, 37.5, 115, 115, 115, 117.5, 117.5, 120, 120, 120, 120];
          data["West-End - Vauxhall"] = [0, 0, 37.5, 70, 37.5, 40, 45, 45, 47.5, 47.5, 50, 55, 55];
          data["West-End - Victoria"] = [0, 0, 70, 60, 70, 72.5, 77.5, 80, 82.5, 82.5, 82.5, 85, 85];

          data["Docklands"] = [38, 38, 31.25, 31.25, 31.75, 32.5, 32.5, 33, 34, 34.5, 34.5, 36.5, 38.33];
          data["Docklands - Canary Wharf"] = [0, 0, 35, 35, 36, 37.5, 37.5, 37.5, 38, 38, 38, 40, 42.5];
          data["Docklands - Other Docklands"] = [0, 0, 27.5, 27.5, 27.5, 27.5, 27.5, 28.5, 30, 31, 31, 32.5, 35];

          var dataset1 = "";
          var dataset2 = "";
          if (Area == 'City' || Area == 'West-End' || Area == 'Docklands') {
            var tmpArea = Area;
            if (Sub == "Canary Wharf")
              tmpArea = "Docklands - Canary Wharf";
            else if (Sub == "Other Docklands")
              tmpArea = "Docklands - Other Docklands";
            else if (Sub == "Victoria")
              tmpArea = "West-End - Victoria";
            else if (Sub == "Vauxhall")
              tmpArea = "West-End - Vauxhall";
            else if (Sub == "St James's")
              tmpArea = "West-End - St James's";
            else if (Sub == "Soho")
              tmpArea = "West-End - Soho";
            else if (Sub == "Paddington")
              tmpArea = "West-End - Paddington";
            else if (Sub == "North of Oxford St/Marylebone")
              tmpArea = "West-End - North of Oxford St/Marylebone";
            else if (Sub == "Mayfair/St James's")
              tmpArea = "West-End - Mayfair/St James's";
            else if (Sub == "Mayfair")
              tmpArea = "West-End - Mayfair";
            else if (Sub == "King's Cross")
              tmpArea = "West-End - King's Cross";
            else if (Sub == "Kensington")
              tmpArea = "West-End - Kensington";
            else if (Sub == "Hammersmith")
              tmpArea = "West-End - Hammersmith";
            else if (Sub == "Fulham")
              tmpArea = "West-End - Fulham";
            else if (Sub == "Fitzrovia")
              tmpArea = "West-End - Fitzrovia";
            else if (Sub == "Euston")
              tmpArea = "West-End - Euston";
            else if (Sub == "Covent Garden")
              tmpArea = "West-End - Covent Garden";
            else if (Sub == "Chelsea")
              tmpArea = "West-End - Chelsea";
            else if (Sub == "Camden")
              tmpArea = "West-End - Camden";
            else if (Sub == "Bloomsbury (WC1)")
              tmpArea = "West-End - Bloomsbury (WC1)";
            else if (Sub == "Belgravia/Knightsbridge")
              tmpArea = "West-End - Belgravia/Knightsbridge";
            else if (Sub == "Battersea")
              tmpArea = "West-End - Battersea";
            else if (Sub == "Clerkenwell")
              tmpArea = "City - Clerkenwell";
            else if (Sub == "Shoreditch")
              tmpArea = "City - Shoreditch";
            else if (Sub == "West City")
              tmpArea = "City - West City";
            else if (Sub == "Waterloo/Bankside")
              tmpArea = "City - Waterloo/Bankside";
            else if (Sub == "London Bridge/More London")
              tmpArea = "City - London Bridge/More London";
            else if (Sub == "Insurance Sector")
              tmpArea = "City - Insurance Sector";
            else if (Sub == "City Northern Fringe")
              tmpArea = "City - City Northern Fringe";
            else if (Sub == "City Eastern Fringe")
              tmpArea = "City - City Eastern Fringe";
            else if (Sub == "Chancery Lane/Midtown")
              tmpArea = "City - Chancery Lane/Midtown";
            else if (Sub == "City Core")
              tmpArea = "City - City Core";

            dataset1 = {
              label: tmpArea,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(179,181,198,0.2)",
              borderColor: "rgba(179,181,198,1)",
              pointBackgroundColor: "rgba(179,181,198,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(179,181,198,1)",
              data: data[tmpArea]
            };
          }
          if (AreaC == 'City' || AreaC == 'West-End' || AreaC == 'Docklands') {
            var tmpAreaC = AreaC;
            if (SubC == "Canary Wharf")
              tmpAreaC = "Docklands - Canary Wharf";
            else if (SubC == "Other Docklands")
              tmpAreaC = "Docklands - Other Docklands";
            else if (SubC == "Victoria")
              tmpAreaC = "West-End - Victoria";
            else if (SubC == "Vauxhall")
              tmpAreaC = "West-End - Vauxhall";
            else if (SubC == "St James's")
              tmpAreaC = "West-End - St James's";
            else if (SubC == "Soho")
              tmpAreaC = "West-End - Soho";
            else if (SubC == "Paddington")
              tmpAreaC = "West-End - Paddington";
            else if (SubC == "North of Oxford St/Marylebone")
              tmpAreaC = "West-End - North of Oxford St/Marylebone";
            else if (SubC == "Mayfair/St James's")
              tmpAreaC = "West-End - Mayfair/St James's";
            else if (SubC == "Mayfair")
              tmpAreaC = "West-End - Mayfair";
            else if (SubC == "King's Cross")
              tmpAreaC = "West-End - King's Cross";
            else if (SubC == "Kensington")
              tmpAreaC = "West-End - Kensington";
            else if (SubC == "Hammersmith")
              tmpAreaC = "West-End - Hammersmith";
            else if (SubC == "Fulham")
              tmpAreaC = "West-End - Fulham";
            else if (SubC == "Fitzrovia")
              tmpAreaC = "West-End - Fitzrovia";
            else if (SubC == "Euston")
              tmpAreaC = "West-End - Euston";
            else if (SubC == "Covent Garden")
              tmpAreaC = "West-End - Covent Garden";
            else if (SubC == "Chelsea")
              tmpAreaC = "West-End - Chelsea";
            else if (SubC == "Camden")
              tmpAreaC = "West-End - Camden";
            else if (SubC == "Bloomsbury (WC1)")
              tmpAreaC = "West-End - Bloomsbury (WC1)";
            else if (SubC == "Belgravia/Knightsbridge")
              tmpAreaC = "West-End - Belgravia/Knightsbridge";
            else if (SubC == "Battersea")
              tmpAreaC = "West-End - Battersea";
            else if (SubC == "Clerkenwell")
              tmpAreaC = "City - Clerkenwell";
            else if (SubC == "Shoreditch")
              tmpAreaC = "City - Shoreditch";
            else if (SubC == "West City")
              tmpAreaC = "City - West City";
            else if (SubC == "Waterloo/Bankside")
              tmpAreaC = "City - Waterloo/Bankside";
            else if (SubC == "London Bridge/More London")
              tmpAreaC = "City - London Bridge/More London";
            else if (SubC == "Insurance Sector")
              tmpAreaC = "City - Insurance Sector";
            else if (SubC == "City Northern Fringe")
              tmpAreaC = "City - City Northern Fringe";
            else if (SubC == "City Eastern Fringe")
              tmpAreaC = "City - City Eastern Fringe";
            else if (SubC == "Chancery Lane/Midtown")
              tmpAreaC = "City - Chancery Lane/Midtown";
            else if (SubC == "City Core")
              tmpAreaC = "City - City Core";
            dataset2 = {
              label: tmpAreaC,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(255,99,132,0.2)",
              borderColor: "rgba(255,99,132,1)",
              pointBackgroundColor: "rgba(255,99,132,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(255,99,132,1)",
              data: data[tmpAreaC]
            };
          }

          if (dataset1 != "" || dataset2 != "") {
            var datasets = new Array();
            if (dataset1 != "")
              datasets.push(dataset1);
            if (dataset2 != "")
              datasets.push(dataset2);

            options = {
              scales: {
                yAxes: [{
                  scaleLabel: {
                    display: true,
                    labelString: 'Price in Pounds, per sq. ft. '
                  }
                }]
              }
            };

            var RentPrime = $("#rent-prime-chart");
            var dataRentPrime = {
              labels: ["2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
              datasets: datasets
            };
            var myLineChart = new Chart(RentPrime, {
              type: 'line',
              data: dataRentPrime,
              options: options
            });
          }
          else {
            $('#RentPrime').hide();
          }

          /****** Rent Occupiers ***************************/

          /*
           labels1 = ["Banking & finance", "Professional", "Creative industries", "Consumer services & leisure", "Business services", "Public sector / Regulatory budies", "Manufacturing, industrial & energy", "Insurance"];
           labels2 = ["Business Services", "Media & Tech", "Financial Services", "Other", "Legal", "Charity", "Transport", "Insurance"];
           labels3 = ["Business Services", "Media & Tech", "Financial Services", "Other", "Legal", "Charity", "Transport", "Retail & Leisure"];
           labels4 = ["Business Services", "Media & Tech", "Financial Services", "Other", "Legal", "Insurance", "Property", "Public Sector", "Retail & Leisure"];
           labels5 = ["Business Services", "Media & Tech", "Financial Services", "Other", "Legal", "Insurance", "Property", "Healthcare", "Energy & Utilities"];
           labels6 = ["Banking & Financial Services", "Media & Tech", "Business Services", "Retail & Leisure", "Energy & Utilities", "Legal", "Healthcare", "Property", "Public Sector", "Other"];
           labels7 = ["Banking & Financial Services", "Media & Tech", "Business Services", "Retail & Leisure", "Energy & Utilities", "Legal", "Healthcare", "Property", "Public Sector", "Other"];
           labels8 = ["Banking & Financial Services", "Media & Tech", "Business Services", "Retail & Leisure", "Energy & Utilities", "Legal", "Healthcare", "Property", "Public Sector", "Other"];

           data = new Array();
           data["City"] = new Array();
           data["City"]["2016-Q1"] = [45, 22, 12, 8, 6, 3, 2, 2];
           data["City"]["2015-Q4"] = [30, 20, 22, 3, 16, 3, 3, 3];
           data["City"]["2015-Q3"] = [24, 8, 22, 5, 29, 5, 2, 5];
           data["City"]["2015-Q2"] = [38, 24, 12, 6, 10, 5, 2, 2, 1];
           data["City"]["2015-Q1"] = [41, 22, 16, 7, 6, 3, 2, 2, 2];
           data["City"]["2014-Q3"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
           data["City"]["2014-Q2"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
           data["City"]["2014-Q1"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
           data["West-End"] = new Array();
           data["West-End"]["2016-Q1"] = [12, 2, 25, 8, 48, 1, 4, 0];
           data["West-End"]["2015-Q4"] = [18, 45, 24, 2, 1, 1, 6, 4];
           data["West-End"]["2015-Q3"] = [21, 45, 24, 1, 1, 2, 4, 2];
           data["West-End"]["2015-Q2"] = [17, 30, 19, 4, 8, 5, 5, 4, 8];
           data["West-End"]["2015-Q1"] = [20, 22, 14, 2, 1, 18, 5, 6, 1];
           data["West-End"]["2014-Q3"] = [23, 22, 20, 10, 5, 5, 5, 4, 4, 2];
           data["West-End"]["2014-Q2"] = [23, 22, 20, 10, 5, 5, 5, 4, 4, 2];
           data["West-End"]["2014-Q1"] = [23, 22, 20, 10, 5, 5, 5, 4, 4, 2];

           var dataset1 = "";
           var dataset2 = "";
           if (Area == 'City' || Area == 'West-End') {
           dataset1 = {
           data: data[Area]["2016-Q1"],
           backgroundColor: [
           "#b3b5c6",
           "#ff6384",
           "#8463ff",
           "#ff63ff",
           "#eb935e",
           "#a3c950",
           "#41a18c",
           "#55419f"
           ],
           hoverBackgroundColor: [
           "#c9cbdc",
           "#e75b78",
           "#7559de",
           "#eb5eeb",
           "#c97e50",
           "#83a141",
           "#317868",
           "#3e2f76"
           ]
           };
           }
           if (AreaC == 'City' || AreaC == 'West-End') {
           dataset2 = {
           data: data[AreaC]["2016-Q1"],
           backgroundColor: [
           "#b3b5c6",
           "#ff6384",
           "#8463ff",
           "#ff63ff",
           "#eb935e",
           "#a3c950",
           "#41a18c",
           "#55419f"
           ],
           hoverBackgroundColor: [
           "#c9cbdc",
           "#e75b78",
           "#7559de",
           "#eb5eeb",
           "#c97e50",
           "#83a141",
           "#317868",
           "#3e2f76"
           ]
           };
           }

           if (dataset1 != "" || dataset2 != "") {
           if (dataset2 == "" || dataset1 == "")
           $('.chartPieOccupiers').removeClass('col-sm-6').addClass('col-sm-12');
           if (dataset1 != "") {
           $('.chartPieOccupiers1').removeClass('hide');
           var RentOccupiers = $("#rent-occupiers-chart");
           var dataRentOccupiers = {
           labels: labels,
           datasets: [dataset1]
           };
           var myLineChart = new Chart(RentOccupiers, {
           type: 'pie',
           data: dataRentOccupiers
           });
           }
           if (dataset2 != "") {
           $('.chartPieOccupiers2').removeClass('hide');
           var RentOccupiers2 = $("#rent-occupiers-chart2");
           var dataRentOccupiers2 = {
           labels : labels,
           datasets: [dataset2]
           };
           var myLineChart = new Chart(RentOccupiers2, {
           type: 'pie',
           data: dataRentOccupiers2
           });
           }

           }

           */

          data = new Array();
          data["City"] = [45, 22, 12, 8, 6, 3, 2, 2];
          data["West-End"] = [12, 2, 25, 8, 48, 1, 4, 0];
          var dataset1 = "";
          var dataset2 = "";
          if (Area == 'City' || Area == 'West-End') {
            dataset1 = {
              data: data[Area],
              backgroundColor: [
                "#b3b5c6",
                "#ff6384",
                "#8463ff",
                "#ff63ff",
                "#eb935e",
                "#a3c950",
                "#41a18c",
                "#55419f"
              ],
              hoverBackgroundColor: [
                "#c9cbdc",
                "#e75b78",
                "#7559de",
                "#eb5eeb",
                "#c97e50",
                "#83a141",
                "#317868",
                "#3e2f76"
              ]
            };
          }
          if (AreaC == 'City' || AreaC == 'West-End') {
            dataset2 = {
              data: data[AreaC],
              backgroundColor: [
                "#b3b5c6",
                "#ff6384",
                "#8463ff",
                "#ff63ff",
                "#eb935e",
                "#a3c950",
                "#41a18c",
                "#55419f"
              ],
              hoverBackgroundColor: [
                "#c9cbdc",
                "#e75b78",
                "#7559de",
                "#eb5eeb",
                "#c97e50",
                "#83a141",
                "#317868",
                "#3e2f76"
              ]
            };
          }

          if (dataset1 != "" || dataset2 != "") {
            if (dataset2 == "" || dataset1 == "")
              $('.chartPieOccupiers').removeClass('col-sm-6').addClass('col-sm-12');
            if (dataset1 != "") {
              $('.chartPieOccupiers1').removeClass('hide');
              var RentOccupiers = $("#rent-occupiers-chart");
              var dataRentOccupiers = {
                labels: ["Banking & finance", "Professional", "Creative industries", "Consumer services & leisure", "Business services", "Public sector / Regulatory budies", "Manufacturing, industrial & energy", "Insurance"],
                datasets: [dataset1]
              };
              var myLineChart = new Chart(RentOccupiers, {
                type: 'pie',
                data: dataRentOccupiers
              });
            }
            if (dataset2 != "") {
              $('.chartPieOccupiers2').removeClass('hide');
              var RentOccupiers2 = $("#rent-occupiers-chart2");
              var dataRentOccupiers2 = {
                labels: ["Banking & finance", "Professional", "Creative industries", "Consumer services & leisure", "Business services", "Public sector / Regulatory budies", "Manufacturing, industrial & energy", "Insurance"],
                datasets: [dataset2]
              };
              var myLineChart = new Chart(RentOccupiers2, {
                type: 'pie',
                data: dataRentOccupiers2
              });
            }

          }
          else {
            $('#RentOccupiersWrapper').hide();
          }

          /****** Investment Stock ***************************/
          data = new Array();
          data["City"] = [456.329, 1057.428, 1729.163, 3.097, 0.492, 1.037, 1.072, 3.302, 0.025];
          data["West-End"] = [581.171, 1119.604, 1038.412, 2.2, -0.264, 0.193, 0.539, 0.685, -0.179];
          data["Docklands"] = [65.223, 245.304, -14.171, 0.398, -0.024, -0.024, 0.1, 0.082, 0.137];
          data["Southbank"] = [190.939, 352.518, 115.107, 0.667, -0.001, -0.01, -0.057, 0.279, 0.052];
          data["Canary Wharf"] = [229.128, 801.598, 148.49, 1.012, -0.157, 0.295, 0.12, 0.593, -0.026];
          var dataset1 = "";
          var dataset2 = "";
          if (Area == 'City' || Area == 'West-End' || Area == 'Docklands' || Area == 'Southbank' || Area == 'Canary Wharf') {
            dataset1 = {
              label: Area,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(179,181,198,0.2)",
              borderColor: "rgba(179,181,198,1)",
              pointBackgroundColor: "rgba(179,181,198,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(179,181,198,1)",
              data: data[Area]
            };
          }
          if (AreaC == 'City' || AreaC == 'West-End' || AreaC == 'Docklands' || AreaC == 'Southbank' || AreaC == 'Canary Wharf') {
            dataset2 = {
              label: AreaC,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(255,99,132,0.2)",
              borderColor: "rgba(255,99,132,1)",
              pointBackgroundColor: "rgba(255,99,132,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(255,99,132,1)",
              data: data[AreaC]
            };
          }

          if (dataset1 != "" || dataset2 != "") {
            var datasets = new Array();
            if (dataset1 != "")
              datasets.push(dataset1);
            if (dataset2 != "")
              datasets.push(dataset2);

            options = {
              scales: {
                yAxes: [{
                  scaleLabel: {
                    display: true,
                    labelString: 'Stock (million sq ft)'
                  }
                }]
              }
            };

            var InvestmentStock = $("#investment-stock-chart");
            var dataInvestmentStock = {
              labels: ["2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
              datasets: datasets
            };
            var myLineChart = new Chart(InvestmentStock, {
              type: 'line',
              data: dataInvestmentStock,
              options: options
            });
          }
          else {
            $('#InvestmentStock').hide();
          }

          /****** Investment Absorption ***************************/
          data = new Array();
          data["City"] = [1.06, 1.60, 1.70, 1.72, 1.53, 1.65, 1.66, 1.21, 0.85, 2.09, 2.18, 1.80, 1.58, 1.37, 1.44, 1.02, 1.56, 1.71, 1.77, 2.27, 2.43, 1.75, 1.24];
          data["West-End"] = [1.1, 2.05, 1.53, 1.79, 1.61, 1.27, 1.54, 1.27, 0.58, 1.38, 1.23, 1.74, 1.74, 1.79, 0.99, 1.44, 1.27, 1.89, 1.63, 1.85, 1.35, 1.54, 1.36];
          var dataset1 = "";
          var dataset2 = "";
          if (Area == 'City' || Area == 'West-End') {
            dataset1 = {
              label: Area,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(179,181,198,0.2)",
              borderColor: "rgba(179,181,198,1)",
              pointBackgroundColor: "rgba(179,181,198,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(179,181,198,1)",
              data: data[Area]
            };
          }
          if (AreaC == 'City' || AreaC == 'West-End') {
            dataset2 = {
              label: AreaC,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(255,99,132,0.2)",
              borderColor: "rgba(255,99,132,1)",
              pointBackgroundColor: "rgba(255,99,132,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(255,99,132,1)",
              data: data[AreaC]
            };
          }

          if (dataset1 != "" || dataset2 != "") {
            var datasets = new Array();
            if (dataset1 != "")
              datasets.push(dataset1);
            if (dataset2 != "")
              datasets.push(dataset2);

            options = {
              scales: {
                yAxes: [{
                  scaleLabel: {
                    display: true,
                    labelString: 'Absorption in M sq. ft'
                  }
                }]
              }
            };

            var InvestmentAbsorption = $("#investment-absorption-chart");
            var dataInvestmentAbsorption = {
              labels: ["2005-Q2", "2005-Q4", "2006-Q2", "2006-Q4", "2007-Q2", "2007-Q4", "2008-Q2", "2008-Q4", "2009-Q2", "2009-Q4", "2010-Q2", "2010-Q4", "2011-Q2", "2011-Q4", "2012-Q2", "2012-Q4", "2013-Q2", "2013-Q4", "2014-Q2", "2014-Q4", "2015-Q2", "2015-Q4", "2016-Q1"],
              datasets: datasets
            };
            var myLineChart = new Chart(InvestmentAbsorption, {
              type: 'line',
              data: dataInvestmentAbsorption,
              options: options
            });
          }
          else {
            $('#InvestmentAbsorption').hide();
          }

          /****** Investment Yields ***************************/
          data = new Array();
          data["City"] = [4.57, 4.56, 4.34, 4.34, 4.36, 4.6, 4.8, 4.99, 5.2, 5.4, 5.61, 5.81, 6, 6.19, 6.37, 6.56, 6.76, 6.88, 6.64, 6.42, 6.2, 5.91, 5.83, 5.64, 5.43, 5.42, 5.43, 5.44, 5.45, 5.46, 5.46, 5.24, 5.09, 4.97, 4.77, 4.66, 4.53, 4.5, 4.31, 4.3, 4.31];
          data["West-End"] = [3.81, 3.82, 3.79, 3.61, 3.62, 4.04, 4.41, 4.61, 4.64, 5.09, 5.4, 5.64, 5.46, 5.03, 4.64, 4.42, 4.33, 4.17, 4.19, 4.19, 4.2, 4.2, 4.21, 4.22, 2.22, 4.22, 4.23, 4.24, 2.24, 4.12, 4.01, 4.01, 4.01, 4.02, 4.02, 4.03, 3.9, 3.8, 3.8, 3.81, 3.81];
          var dataset1 = "";
          var dataset2 = "";
          if (Area == 'City' || Area == 'West-End') {
            dataset1 = {
              label: Area,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(179,181,198,0.2)",
              borderColor: "rgba(179,181,198,1)",
              pointBackgroundColor: "rgba(179,181,198,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(179,181,198,1)",
              data: data[Area]
            };
          }
          if (AreaC == 'City' || AreaC == 'West-End') {
            dataset2 = {
              label: AreaC,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(255,99,132,0.2)",
              borderColor: "rgba(255,99,132,1)",
              pointBackgroundColor: "rgba(255,99,132,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(255,99,132,1)",
              data: data[AreaC]
            };
          }

          if (dataset1 != "" || dataset2 != "") {
            var datasets = new Array();
            if (dataset1 != "")
              datasets.push(dataset1);
            if (dataset2 != "")
              datasets.push(dataset2);

            options = {
              scales: {
                yAxes: [{
                  scaleLabel: {
                    display: true,
                    labelString: 'Prime Yield %'
                  }
                }]
              }
            };

            var InvestmentYields = $("#investment-yields-chart");
            var dataInvestmentYields = {
              labels: ["2006-Q1", "2006-Q2", "2006-Q3", "2006-Q4", "2007-Q1", "2007-Q2", "2007-Q3", "2007-Q4", "2008-Q1", "2008-Q2", "2008-Q3", "2008-Q4", "2009-Q1", "2009-Q2", "2009-Q3", "2009-Q4", "2010-Q1", "2010-Q2", "2010-Q3", "2010-Q4", "2011-Q1", "2011-Q2", "2011-Q3", "2011-Q4", "2012-Q1", "2012-Q2", "2012-Q3", "2012-Q4", "2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
              datasets: datasets
            };
            var myLineChart = new Chart(InvestmentYields, {
              type: 'line',
              data: dataInvestmentYields,
              options: options
            });
          }
          else {
            $('#InvestmentYields').hide();
          }

          /****** Investment Sell ***************************/
          data = new Array();
          data["City"] = [2518, 3221, 2157, 2071, 2499, 3410, 3116, 970, 1388, 1074, 457, 315, 476, 827, 1768, 675, 580, 1131, 1654, 1768, 1236, 2033, 1293, 1445, 1616, 2603, 1834, 1919, 1150, 1350, 2138, 5224, 989, 2185, 1957, 2983, 1768, 2679, 1758, 2613, 1150];
          data["West-End"] = [968, 712, 1377, 1871, 1358, 1662, 2127, 617, 1063, 589, 551, 322, 380, 589, 1234, 798, 808, 1453, 892, 1215, 1310, 846, 978, 940, 1510, 1301, 1358, 1121, 1624, 1804, 2583, 1662, 1196, 1178, 1634, 1994, 1149, 1653, 1842, 1947, 1453];
          data["Docklands"] = [466, 0, 0, 295, 47, 1216, 1643, 38, 0, 76, 0, 1093, 0, 0, 0, 969, 237, 190, 48, 513, 0, 0, 0, 57, 465, 181, 0, 48, 570, 57, 228, 0, 285, 817, 513, 1178, 0, 66, 228, 246, 646];
          var dataset1 = "";
          var dataset2 = "";
          if (Area == 'City' || Area == 'West-End' || Area == 'Docklands') {
            dataset1 = {
              label: Area,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(179,181,198,0.2)",
              borderColor: "rgba(179,181,198,1)",
              pointBackgroundColor: "rgba(179,181,198,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(179,181,198,1)",
              data: data[Area]
            };
          }
          if (AreaC == 'City' || AreaC == 'West-End' || AreaC == 'Docklands') {
            dataset2 = {
              label: AreaC,
              fill: false,
              lineTension: 0.1,
              backgroundColor: "rgba(255,99,132,0.2)",
              borderColor: "rgba(255,99,132,1)",
              pointBackgroundColor: "rgba(255,99,132,1)",
              pointBorderColor: "#fff",
              pointHoverBackgroundColor: "#fff",
              pointHoverBorderColor: "rgba(255,99,132,1)",
              data: data[AreaC]
            };
          }

          if (dataset1 != "" || dataset2 != "") {
            var datasets = new Array();
            if (dataset1 != "")
              datasets.push(dataset1);
            if (dataset2 != "")
              datasets.push(dataset2);

            options = {
              scales: {
                yAxes: [{
                  scaleLabel: {
                    display: true,
                    labelString: 'Investment in M pounds'
                  }
                }]
              }
            };

            var InvestmentSell = $("#investment-sell-chart");
            var dataInvestmentSell = {
              labels: ["2006-Q1", "2006-Q2", "2006-Q3", "2006-Q4", "2007-Q1", "2007-Q2", "2007-Q3", "2007-Q4", "2008-Q1", "2008-Q2", "2008-Q3", "2008-Q4", "2009-Q1", "2009-Q2", "2009-Q3", "2009-Q4", "2010-Q1", "2010-Q2", "2010-Q3", "2010-Q4", "2011-Q1", "2011-Q2", "2011-Q3", "2011-Q4", "2012-Q1", "2012-Q2", "2012-Q3", "2012-Q4", "2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
              datasets: datasets
            };
            var myLineChart = new Chart(InvestmentSell, {
              type: 'line',
              data: dataInvestmentSell,
              options: options
            });
          }
          else {
            $('#InvestmentSell').hide();
          }

          /****** Investment Purchaser ***************************/
          data = new Array();
          data["Central London"] = [11, 12, 6, 6, 10, 13, 28, 11, 3];
          var dataset1 = "";
          var dataset2 = "";
          if (Area == 'Central London') {
            dataset1 = {
              data: data[Area],
              backgroundColor: [
                "#b3b5c6",
                "#ff6384",
                "#8463ff",
                "#ff63ff",
                "#eb935e",
                "#a3c950",
                "#41a18c",
                "#55419f"
              ],
              hoverBackgroundColor: [
                "#c9cbdc",
                "#e75b78",
                "#7559de",
                "#eb5eeb",
                "#c97e50",
                "#83a141",
                "#317868",
                "#3e2f76"
              ]
            };
          }
          if (AreaC == 'Central London') {
            dataset2 = {
              data: data[AreaC],
              backgroundColor: [
                "#b3b5c6",
                "#ff6384",
                "#8463ff",
                "#ff63ff",
                "#eb935e",
                "#a3c950",
                "#41a18c",
                "#316278",
                "#55419f"
              ],
              hoverBackgroundColor: [
                "#c9cbdc",
                "#e75b78",
                "#7559de",
                "#eb5eeb",
                "#c97e50",
                "#83a141",
                "#317868",
                "#41819f",
                "#3e2f76"
              ]
            };
          }
          if (dataset1 != "" || dataset2 != "") {
            if (dataset2 == "" || dataset1 == "")
              $('.chartPiePurchaser').removeClass('col-sm-6').addClass('col-sm-12');
            if (dataset1 != "") {
              $('.chartPiePurchaser1').removeClass('hide');
              var InvestmentPurchaser = $("#investment-purchaser-chart");
              var dataInvestmentPurchaser = {
                labels: ["UK Institutions", "UK Prop Co", "UK Other", "USA / Canada", "Middle East / North Africa", "Europe", "Asia", "Overseas Other", "Unknown"],
                datasets: [dataset1]
              };
              var myLineChart = new Chart(InvestmentPurchaser, {
                type: 'pie',
                data: dataInvestmentPurchaser
              });
            }
            if (dataset2 != "") {
              $('.chartPiePurchaser2').removeClass('hide');
              var InvestmentPurchaser2 = $("#investment-purchaser-chart2");
              var dataInvestmentPurchaser2 = {
                labels: ["UK Institutions", "UK Prop Co", "UK Other", "USA / Canada", "Middle East / North Africa", "Europe", "Asia", "Overseas Other", "Unknown"],
                datasets: [dataset2]
              };
              var myLineChart = new Chart(InvestmentPurchaser2, {
                type: 'pie',
                data: dataInvestmentPurchaser2
              });
            }

          }
          else {
            $('#InvestmentPurchaser').hide();
          }


          /****** Constructions Under ***************************/
          data = new Array();
          data["City"] = new Array();
          data["City"]["Completed"] = [5.03, 3.03, 3.03, 3.06, 1.06, 0.56, 2.44, 3.54, 3.06, 2.83, 1.15, 1.06, 0.86, 3.55, 1.96, 0.29, 0, 0, 0];
          data["City"]["Speculative"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.64, 1.23, 0.93, 0];
          data["City"]["Permitted"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.62, 1.59, 2.13];
          data["City"]["Pre-let"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.87, 2.47, 0.5, 0];
          data["West-End"] = new Array();
          data["West-End"]["Completed"] = [1.11, 1.11, 1.11, 2.7, 1.01, 0.81, 0.7, 2, 1.2, 2.21, 0.63, 1.01, 1.42, 1.82, 1.43, 0.32, 0, 0, 0];
          data["West-End"]["Speculative"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.3, 0.52, 0, 0];
          data["West-End"]["Permitted"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.61, 1.43, 0.71];
          data["West-End"]["Pre-let"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5, 0.69, 0.19, 0];
          data["Docklands"] = new Array();
          data["Docklands"]["Speculative"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.7, 0];
          data["Central London"] = new Array();
          data["Central London"]["Completed"] = [1.49, 1.68, 1.85, 2.99, 2.82, 3.99, 5.51, 5.21, 6.14, 10.21, 4.67, 1.98, 2.9, 4.02, 5.08, 6.54, 4.05, 1.66, 2.28, 3.5, 5.89, 3.37, 0.95, 0, 0, 0, 0];
          data["Central London"]["U/C Let / Under Offer"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2.47, 1.87, 1.36, 0.35, 0];
          data["Central London"]["UC"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3.23, 2.55, 1.6, 1.03, 0];
          data["Central London"]["Proposed Available"] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.14, 1.36, 5.81, 6.16, 4.7];

          var labels = "";
          var labels1 = ["2001", "2002", "2003", "2004", "2005", "2006", "2007", "2008", "2009", "2010", "2011", "2012", "2013", "2014", "2015", "2016", "2017", "2018", "2019"];
          var labels2 = ["1994", "1995", "1996", "1997", "1998", "1999", "2000", "2001", "2002", "2003", "2004", "2005", "2006", "2007", "2008", "2009", "2010", "2011", "2012", "2013", "2014", "2015", "2016", "2017", "2018", "2019", "2020"];

          var dataset1 = "";
          var dataset2 = "";
          if (Area == 'City' || Area == 'West-End' || Area == 'Docklands') {
            labels = labels1;
            dataset1 = [
              {
                label: "Completed",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#55419f",
                borderColor: "#3e2f76",
                pointBackgroundColor: "rgba(179,181,198,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(179,181,198,1)",
                data: data[Area]["Completed"]
              },
              {
                label: "Speculative",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#83a141",
                borderColor: "#a3c950",
                pointBackgroundColor: "rgba(255,99,132,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,132,1)",
                data: data[Area]["Speculative"]
              },
              {
                label: "Permitted",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#eb935e",
                borderColor: "#eb935e",
                pointBackgroundColor: "rgba(132,99,255,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(132,99,255,1)",
                data: data[Area]["Permitted"]
              },
              {
                label: "Pre-let",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#e75b78",
                borderColor: "#e75b78",
                pointBackgroundColor: "rgba(255,99,255,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,255,1)",
                data: data[Area]["Pre-let"]
              }
            ];
          }
          else if (Area == 'Central London') {
            labels = labels2;
            dataset1 = [
              {
                label: "Completed",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#55419f",
                borderColor: "#3e2f76",
                pointBackgroundColor: "rgba(179,181,198,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(179,181,198,1)",
                data: data[Area]["Completed"]
              },
              {
                label: "U/C Let / Under Offer",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#83a141",
                borderColor: "#a3c950",
                pointBackgroundColor: "rgba(255,99,132,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,132,1)",
                data: data[Area]["U/C Let / Under Offer"]
              },
              {
                label: "U/C",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#eb935e",
                borderColor: "#eb935e",
                pointBackgroundColor: "rgba(132,99,255,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(132,99,255,1)",
                data: data[Area]["U/C"]
              },
              {
                label: "Proposed Available",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#e75b78",
                borderColor: "#e75b78",
                pointBackgroundColor: "rgba(255,99,255,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,255,1)",
                data: data[Area]["Proposed Available"]
              }
            ];
          }
          if (AreaC == 'City' || AreaC == 'West-End' || AreaC == 'Docklands') {
            labels = labels1;
            dataset2 = [
              {
                label: "Completed",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#55419f",
                borderColor: "#3e2f76",
                pointBackgroundColor: "rgba(179,181,198,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(179,181,198,1)",
                data: data[AreaC]["Completed"]
              },
              {
                label: "Speculative",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#83a141",
                borderColor: "#a3c950",
                pointBackgroundColor: "rgba(255,99,132,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,132,1)",
                data: data[AreaC]["Speculative"]
              },
              {
                label: "Permitted",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#eb935e",
                borderColor: "#eb935e",
                pointBackgroundColor: "rgba(132,99,255,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(132,99,255,1)",
                data: data[AreaC]["Permitted"]
              },
              {
                label: "Pre-let",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#e75b78",
                borderColor: "#e75b78",
                pointBackgroundColor: "rgba(255,99,255,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,255,1)",
                data: data[AreaC]["Pre-let"]
              }
            ];
          }
          else if (Area == 'Central London') {
            labels = labels2;
            dataset2 = [
              {
                label: "Completed",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#55419f",
                borderColor: "#3e2f76",
                pointBackgroundColor: "rgba(179,181,198,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(179,181,198,1)",
                data: data[Area]["Completed"]
              },
              {
                label: "U/C Let / Under Offer",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#83a141",
                borderColor: "#a3c950",
                pointBackgroundColor: "rgba(255,99,132,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,132,1)",
                data: data[Area]["U/C Let / Under Offer"]
              },
              {
                label: "U/C",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#eb935e",
                borderColor: "#eb935e",
                pointBackgroundColor: "rgba(132,99,255,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(132,99,255,1)",
                data: data[Area]["U/C"]
              },
              {
                label: "Proposed Available",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#e75b78",
                borderColor: "#e75b78",
                pointBackgroundColor: "rgba(255,99,255,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,255,1)",
                data: data[Area]["Proposed Available"]
              }
            ];
          }

          if (dataset1 != "" || dataset2 != "") {
            if (dataset2 == "" || dataset1 == "")
              $('.chartBarUnder').removeClass('col-sm-6').addClass('col-sm-12');
            if (dataset1 != "") {
              $('.chartBarUnder1').removeClass('hide');

              options = {
                scales: {
                  yAxes: [{
                    scaleLabel: {
                      display: true,
                      labelString: 'Development in M sq. ft.'
                    }
                  }]
                }
              };

              var ConstructionsUnder = $("#constructions-under-chart");
              var dataConstructionsUnder = {
                labels: labels,
                datasets: dataset1
              };
              var myLineChart = new Chart(ConstructionsUnder, {
                type: 'bar',
                data: dataConstructionsUnder,
                options: options
              });
            }
            if (dataset2 != "") {
              $('.chartBarUnder2').removeClass('hide');

              options = {
                scales: {
                  yAxes: [{
                    scaleLabel: {
                      display: true,
                      labelString: 'Development in M sq. ft.'
                    }
                  }]
                }
              };

              var ConstructionsUnder = $("#constructions-under-chart2");
              var dataConstructionsUnder = {
                labels: labels,
                datasets: dataset2
              };
              var myLineChart = new Chart(ConstructionsUnder, {
                type: 'bar',
                data: dataConstructionsUnder,
                options: options
              });
            }

          }
          else {
            $('#ConstructionsUnder').hide();
          }

          /****** Constructions Planning ***************************/
          data = new Array();
          data["Camden"] = [126, 131, 118, 132, 117, 133, 111];
          data["City of London"] = [381, 439, 155, 118, 79, 93, 115];
          data["Hackney"] = [29, 19, 13, 7, 22, 13, 13];
          data["Hammersmith and Fulham"] = [37, 51, 26, 4, 13, 15, 20];
          data["Islington"] = [62, 58, 25, 14, 32, 32, 18];
          data["Kensington and Chelsea"] = [27, 25, 22, 10, 3, 6, 4];
          data["Lambeth"] = [30, 38, 48, 11, 4, 7, 4];
          data["Newham"] = [8, 2, 8, 2, 3, 1, 0];
          data["Southwark"] = [52, 65, 44, 36, 39, 33, 26];
          data["Tower Hamlets"] = [26, 25, 30, 13, 19, 31, 46];
          data["Wandsworth"] = [58, 50, 38, 31, 31, 27, 34];

          var labels = ["2006-2007", "2007-2008", "2008-2009", "2012-2013", "2012-2013", "2014-2015", "2014-2015"];

          var dataset1 = "";
          var dataset2 = "";
          if (District == 'Camden' || District == 'City of London' || District == 'Hackney' || District == 'Hammersmith and Fulham' || District == 'Islington' || District == 'Kensington and Chelsea' || District == 'Lambeth' || District == 'Newham' || District == 'Southwark' || District == 'Tower Hamlets' || District == 'Wandsworth') {
            dataset1 = [
              {
                label: District,
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#e75b78",
                borderColor: "#e75b78",
                pointBackgroundColor: "rgba(255,99,255,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,255,1)",
                data: data[District]
              }
            ];
          }
          if (DistrictC == 'Camden' || DistrictC == 'City of London' || DistrictC == 'Hackney' || DistrictC == 'Hammersmith and Fulham' || DistrictC == 'Islington' || DistrictC == 'Kensington and Chelsea' || DistrictC == 'Lambeth' || DistrictC == 'Newham' || DistrictC == 'Southwark' || DistrictC == 'Tower Hamlets' || DistrictC == 'Wandsworth') {
            dataset2 = [
              {
                label: DistrictC,
                fill: false,
                lineTension: 0.1,
                backgroundColor: "#e75b78",
                borderColor: "#e75b78",
                pointBackgroundColor: "rgba(255,99,255,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,255,1)",
                data: data[DistrictC]
              }
            ];
          }

          if (dataset1 != "" || dataset2 != "") {
            if (dataset2 == "" || dataset1 == "")
              $('.chartBarPlanning').removeClass('col-sm-6').addClass('col-sm-12');
            if (dataset1 != "") {
              $('.chartBarPlanning1').removeClass('hide');

              options = {
                scales: {
                  yAxes: [{
                    scaleLabel: {
                      display: true,
                      labelString: 'Decisions #'
                    }
                  }]
                }
              };

              var ConstructionsPlanning = $("#constructions-planning-chart");
              var dataConstructionsPlanning = {
                labels: labels,
                datasets: dataset1
              };
              var myLineChart = new Chart(ConstructionsPlanning, {
                type: 'bar',
                data: dataConstructionsPlanning,
                options: options
              });
            }
            if (dataset2 != "") {
              $('.chartBarPlanning2').removeClass('hide');

              options = {
                scales: {
                  yAxes: [{
                    scaleLabel: {
                      display: true,
                      labelString: 'Decisions #'
                    }
                  }]
                }
              };

              var ConstructionsPlanning = $("#constructions-planning-chart2");
              var dataConstructionsPlanning = {
                labels: labels,
                datasets: dataset2
              };
              var myLineChart = new Chart(ConstructionsPlanning, {
                type: 'bar',
                data: dataConstructionsPlanning,
                options: options
              });
            }

          }
          else {
            $('#ConstructionsPlanning').hide();
          }


          /****** dataTable ***************************/

          $timeout(function () {
            var oTable1 = $('#dynamic-table').dataTable({
              bAutoWidth: false,
              "aoColumns": [
                null, null, null, null, null, null, null
              ],
              "aaSorting": []
            });
            var oTable2 = $('#dynamic-table2').dataTable({
              bAutoWidth: false,
              "aoColumns": [
                null, null, null, null, null, null
              ],
              "aaSorting": []
            });
            $(window).resize();
          }, 100);

        }, 100);

    };

    $scope.setSearchParameter = function(type){

      if(type === 1) {
          $scope.parameters.Area = $scope.selected ? $scope.selected.value : '';
          $scope.parameters.Sub = $scope.selected ? $scope.selected.sub : '';
          $scope.parameters.District = $scope.selected ? $scope.selected.district : '';
        } else if(type === 2) {
          $scope.parameters.AreaC = $scope.selectedC ? $scope.selectedC.value : '';
          $scope.parameters.SubC = $scope.selectedC ? $scope.selectedC.sub : '';
          $scope.parameters.DistrictC = $scope.selectedC ? $scope.selectedC.district : '';
        } else if(type ==="Type"){
          $scope.parameters.PropertyType = $scope.selectedType ? $scope.selectedType : "";

      }
    };

    $http.get('assets/data/options.json')
      .success(function(result){
        $scope.options = result.options;
        $scope.types = result.type.split(',');

        //$('#Area').on('change', $scope.setSearchParameter(1,''))
    }).error(function(result){
        console.log(result);
    });
  });
