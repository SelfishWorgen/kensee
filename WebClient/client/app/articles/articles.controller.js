'use strict';

String.prototype.trunc = String.prototype.trunc ||
  function(n){
    return this.length>n ? this.substr(0,n-3)+'...' : this;
  };

var multiValueSearch = function(searchTerm, cellValue) {
  if (cellValue === undefined)
  {
    return false;
  }
  var separators = ['-', '/', ':', ';', ','];
  var strippedValue = searchTerm.split(new RegExp(separators.join('|'), 'g'));
  var bReturnValue = false;
  for(var iIndex in strippedValue){
    var sValueToTest = strippedValue[iIndex];
    if (cellValue.toLowerCase().indexOf(sValueToTest.toLowerCase()) >= 0)
      bReturnValue = true;
  }
  return bReturnValue;
};

angular.module('kenseeApp')
  .controller('ArticlesCtrl', function ($scope, $http, uiGridConstants, uiGridExporterConstants, $window, $document, SERVER_URL) {
    $scope.gridBusy = $http.get(SERVER_URL + 'articles')
      .success(function(data) {
        $scope.articlesData = data;
        $scope.articlesData = $scope.articlesData.map(function(article){
          article['date'] = moment(article['date'],"DD-MM-YYYY").toDate();
          return article;
        });
        $scope.gridOptions.data = $scope.articlesData;
        $scope.dateRangeSelected($scope.dateRange); //for the pagination to get refreshed
      });


    $scope.DATE_COL = 1;
    $scope.COMPANY_COL = 3;
    $scope.TOPIC_COL = 4;
    $scope.COUNTRY_COL = 5;
    $scope.CITY_COL = 6;
    $scope.PROPERTY_COL = 7;
    $scope.SENTIMENT_COL = 8;
    $scope.SOURCE_COL = 9;

    //not sure I need to init this but what the hack...
    $scope.article =  {
      url: "",
      title: "",
      date: "",
      content: ""
    };
    $scope.DisplayArticle=false;
    $scope.gridOptions = {
      enableFiltering: true,
      enableRowSelection: true,
      enableSelectAll: true,
      selectionRowHeaderWidth: 30,
      rowHeight: 54,
      headerRowHeight: 40,
      enableColumnResizing: false,  //it creates UX bugs. skip it for now
      enableColumnMenus: false,
      exporterCsvFilename: 'articles.csv',
      enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
      enableVerticalScrollbar: uiGridConstants.scrollbars.NEVER,
      paginationPageSize: 20,
      paginationPageSizes: [],
      rowTemplate: '<div ng-click="grid.appScope.showArticle(row)" ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.uid" class="ui-grid-cell row-bottom-border" ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader }"  ui-grid-cell></div>'
    };

    $scope.gridOptions.columnDefs = [
      { name: 'date', displayName: 'DATE', cellFilter: 'date:\'dd MMM yyyy\'', width: 100, filters: [
        {
          condition: uiGridConstants.filter.GREATER_THAN
        },
        {
          condition: uiGridConstants.filter.LESS_THAN
        }],
        sort: {
          direction: uiGridConstants.DESC,
          priority: 0
        },
        sortingAlgorithm: function (aDate, bDate) {
          //debugger;
          var a=new Date(aDate);
          var b=new Date(bDate);
          if (a < b) {
            return -1;
          }
          else if (a > b) {
            return 1;
          }
          else {
            return 0;
          }
        }
      },
      {
         name: 'title', displayName: 'HEADLINE'
          , cellTemplate: '<div tooltip="{{ row.entity.snippet }}" tooltip-popup-delay="500" tooltip-placement="top" tooltip-append-to-body="true"><div class="ui-grid-cell-contents title-column">{{ row.entity.title }}</div></div>'
      },
      { name: 'company', displayName: 'COMPANY', visible: false, filter: { condition: multiValueSearch } },
      { name: 'topic', displayName: 'TOPIC', visible: false, filter: {condition:multiValueSearch}},
      { name: 'country', displayName: 'COUNTRY', visible: false, filter: {condition:multiValueSearch}},
      { name: 'city', displayName: 'CITY', visible: false, filter: {condition:multiValueSearch}},
      { name: 'property', displayName: 'PROPERTY', visible: false, filter: {condition:multiValueSearch}},
      { name: 'sentiment', displayName: 'SENTIMENT', visible: false, filter: {condition:multiValueSearch}},
      { name: 'source', displayName: 'SOURCE', width: 160, filter: {condition:multiValueSearch}},
 //     { name: 'article_id', displayName: 'ID', width: 55 },  // for testing and development server only
      { name: 'url', displayName: 'URL', visible: false }
    ];

    $scope.gridOptions.exporterFieldCallback = function ( grid, row, col, value ){
      if ( col.name === 'date' ){
        var date = moment(value);
        value = date.format('DD/MM/YYYY');
      }
      return value;
    }
    $scope.endDate = moment(); //now
    $scope.startDate = moment().subtract(1, "month");
    $scope.gridOptions.multiSelect = true;

    $scope.dateRange = "last_month";
    $scope.dateRangeSelected = function(value)
    {
      $scope.endDate = moment(); //in all cases endDate is today
      if (value === "last_week")
      {
        $scope.startDate = moment().subtract(1, "week");
      }
      else if (value === "last_month")
      {
        $scope.startDate = moment().subtract(1, "month");
      }
      else if (value === "last_3_months")
      {
        $scope.startDate = moment().subtract(3, "month");
      }
      else if (value === "last_6_months")
      {
        $scope.startDate = moment().subtract(6, "month");
      }
      else if (value === "last_year")
      {
        $scope.startDate = moment().subtract(1, "year");
      }
    }

    function truncateTags(element) {
      var tags_max_size = 22;
      return String(element).trunc(tags_max_size);
    }
    $scope.prepareTags = function(entity)
    {
      var tagsArray = [];
      if (entity.country)
      {
        tagsArray.push.apply(tagsArray, entity.country.split(',').map(truncateTags));
      }
      if (entity.city)
      {
          var y = entity.city.split('|').map(truncateTags);
          for (var i = 0; i < y.length; i++)
          {
              var nm = y[i];
            if (tagsArray.indexOf(nm) == -1)
              tagsArray.push.apply(tagsArray, [nm]);
          }
//        tagsArray.push.apply(tagsArray, entity.city.split('|').map(truncateTags));
      }
      if (entity.property)
      {
        tagsArray.push.apply(tagsArray, entity.property.split(',').map(truncateTags));
      }
      if (entity.topic)
      {
        tagsArray.push.apply(tagsArray, entity.topic.split(',').map(truncateTags));
      }
      if (entity.company)
      {
        tagsArray.push.apply(tagsArray, entity.company.split('|').map(truncateTags));
      }
      if (entity.sentiment)
      {
        tagsArray.push.apply(tagsArray, entity.sentiment.split(',').map(truncateTags));
      }
      tagsArray = tagsArray.filter(function(n){ return n !== "" });
      return tagsArray;
    }

    $scope.showArticle = function(rowItem)
    {
      $http.get(SERVER_URL + 'articles/' + rowItem.entity.article_id)
        .success(function(data) {
          $scope.article.url = data.url;
          $scope.article.short_url = data.url.trunc(30);
          $scope.article.title = data.title;
          $scope.article.content = data.content;
          $scope.article.date = data.date;
          $scope.article.tags = $scope.prepareTags(rowItem.entity);
          $scope.DisplayArticle = true;
          //it's a trick. articles-div still isn't visible, but grid-main is in the same height...
          var articleElement = angular.element(document.getElementById('grid-main-div'));
          $document.duScrollTo(articleElement,0,250);
        });
    }

    $scope.gridOptions.onRegisterApi = function (gridApi) {
      $scope.gridApi = gridApi;
    }

    $scope.$watch("startDate", function(newValue, oldValue){
      $scope.gridApi.grid.columns[$scope.DATE_COL].filters[0].term = newValue;
      $scope.gridRefresh();
    });

    $scope.$watch("endDate", function(newValue, oldValue){
      $scope.gridApi.grid.columns[$scope.DATE_COL].filters[1].term = newValue;
      $scope.gridRefresh();
    });

    $scope.$watch("gridApi.grid.columns[CITY_COL].filters[0].term", function(newValue, oldValue){
      $scope.reconstructFilterTags();
    });

    $scope.$watch("gridApi.grid.columns[COMPANY_COL].filters[0].term", function(newValue, oldValue){
      $scope.reconstructFilterTags();
    });

    $scope.$watch("gridApi.grid.columns[SOURCE_COL].filters[0].term", function(newValue, oldValue){
      $scope.reconstructFilterTags();
    });

    $scope.closeArticle = function(){
      $scope.DisplayArticle = false;
    };

    $scope.selected_countries = [];
    $scope.countries = [
      'Brazil',
      'Canada',
      'Germany',
      'Norway',
      'Spain',
      'Sweden',
      'Switzerland',
      'United Kingdom',
      'United States'
      ];

    $scope.selected_propertyTypes = [];
    $scope.propertyTypes =
    [ 'Office',
      'Industrial',
      'Retail',
      'Land',
      'Multifamily',
      'Leisure',
      'Healthcare'
    ];

    $scope.selected_sentiments = [];
    $scope.sentiments = ['Positive',
      'Neutral',
      'Negative'
    ];

    $scope.selected_topics = [];
    $scope.topics = ['Buy/Sell',
      'Rent',
      'Construction',
      'Financial update',
      'Revenue update'
    ];

    $scope.filterListChanged = function(selectedArray, column_num)
    {
      if (selectedArray.length == 0)
      {
        $scope.gridApi.grid.columns[column_num].filters[0].term = "";
      }
      else {
          var str = "";
          for (var x in selectedArray)
          {
              if (str != "")
                  str += ",";
              if (selectedArray[x] == "Revenue update") {
                  str += "Revenue increase,";
                  str += "Revenue decrease";
              }
              else {
                  str += selectedArray[x];
              }
          }
          $scope.gridApi.grid.columns[column_num].filters[0].term = str; // arr.join(',');
      }
      $scope.gridRefresh();

      $scope.reconstructFilterTags();

    };

    $scope.filterTags = [];

    //it is a bit easier and more safe to reconstruct the tags rather than kepping double booking of the active filters
    $scope.reconstructFilterTags = function()
    {
      var new_filter_tags = [];
      $scope.selected_countries.forEach(function(country) {
        new_filter_tags.push({'value': country, 'type': 'country'});
      });
      var cityFilter = $scope.gridApi.grid.columns[$scope.CITY_COL].filters[0].term;
      if (cityFilter)
      {
        new_filter_tags.push({'value': cityFilter, 'type': 'city'});
      }
      $scope.selected_propertyTypes.forEach(function(property) {
        new_filter_tags.push({'value': property, 'type': 'property'});
      });
      $scope.selected_topics.forEach(function(topic) {
        new_filter_tags.push({'value': topic, 'type': 'topic'});
      });
      var sourceFilter = $scope.gridApi.grid.columns[$scope.SOURCE_COL].filters[0].term;
      if (sourceFilter)
      {
        new_filter_tags.push({'value': sourceFilter, 'type': 'source'});
      }
      var companyFilter = $scope.gridApi.grid.columns[$scope.COMPANY_COL].filters[0].term;
      if (companyFilter)
      {
        new_filter_tags.push({'value': companyFilter, 'type': 'company'});
      }
      $scope.selected_sentiments.forEach(function(sentiment) {
        new_filter_tags.push({'value': sentiment, 'type': 'sentiment'});
      });

      $scope.filterTags = new_filter_tags;
    };

    $scope.deleteFilterTag = function(tag_index)
    {
      var value = $scope.filterTags[tag_index].value;
      var type = $scope.filterTags[tag_index].type;

      switch(type) {
        case 'country':
              var index = $scope.selected_countries.indexOf(value);
              if (index > -1) {
                $scope.selected_countries.splice(index, 1);
              }
              else
              {
                console.log('trying to delete a non existant country:' + value);
                return;
              }
              break;
        case 'city':
          $scope.gridApi.grid.columns[$scope.CITY_COL].filters[0].term = "";
          break;
        case 'property':
          var index = $scope.selected_propertyTypes.indexOf(value);
          if (index > -1) {
            $scope.selected_propertyTypes.splice(index, 1);
          }
          else
          {
            console.log('trying to delete a non existant property:' + value);
            return;
          }
          break;
        case 'topic':
          var index = $scope.selected_topics.indexOf(value);
          if (index > -1) {
            $scope.selected_topics.splice(index, 1);
          }
          else
          {
            console.log('trying to delete a non existant topic:' + value);
            return;
          }
          break;
        case 'source':
          $scope.gridApi.grid.columns[$scope.SOURCE_COL].filters[0].term = "";
          break;
        case 'company':
          $scope.gridApi.grid.columns[$scope.COMPANY_COL].filters[0].term = "";
          break;
        case 'sentiment':
          var index = $scope.selected_sentiments.indexOf(value);
          if (index > -1) {
            $scope.selected_sentiments.splice(index, 1);
          }
          else
          {
            console.log('trying to delete a non existant sentiment:' + value);
            return;
          }
          break;
        default:
          return;
      }
      $scope.gridRefresh();
      $scope.reconstructFilterTags();
    };

    $scope.clearAllFilterTags = function()
    {
      $scope.gridApi.grid.columns[$scope.CITY_COL].filters[0].term = "";
      $scope.gridApi.grid.columns[$scope.SOURCE_COL].filters[0].term = "";
      $scope.gridApi.grid.columns[$scope.COMPANY_COL].filters[0].term = "";
      angular.copy([], $scope.selected_countries); //don't do $scope.selected_countries=[]. open bug in checklist-model
      angular.copy([], $scope.selected_propertyTypes);
      angular.copy([], $scope.selected_sentiments);
      angular.copy([], $scope.selected_topics);

      $scope.gridRefresh();
      $scope.reconstructFilterTags();

    }

    $scope.gridRefresh = function()
    {
      $scope.gridApi.grid.queueGridRefresh();
    };

    $scope.clearSearch = function(col_num)
    {
      $scope.gridApi.grid.columns[col_num].filters[0].term = "";
      $scope.gridRefresh();
    };

    $scope.clearLocation = function()
    {
      $scope.gridApi.grid.columns[$scope.CITY_COL].filters[0].term = "";
      angular.copy([], $scope.selected_countries); //don't do $scope.selected_countries=[]. open bug in checklist-model
      $scope.gridRefresh();
    };

    $scope.allSelected=false;

    $scope.exportToCSV = function()
    {
      if ($scope.allSelected)
      {
        $scope.gridApi.exporter.csvExport(uiGridExporterConstants.ALL, uiGridExporterConstants.ALL);
      }
      else {
        $scope.gridApi.exporter.csvExport(uiGridExporterConstants.SELECTED, uiGridExporterConstants.ALL);
      }
    }
    $scope.searchTerm = "";
    $scope.search = function()
    {
      $scope.searchTerm
    }
    $scope.countryCollapsed = true;
    $scope.tagsCollapsed = true;
    $scope.cityCollapsed = true;
    $scope.propertyCollapsed = true;
    $scope.sentimentCollapsed = true;
    $scope.topicCollapsed = true;
    $scope.companyCollapsed = true;
    $scope.sourceCollapsed = true;
    $scope.search = function()
    {

      var request;
      if ($scope.searchTerm === "")
      {
        request = SERVER_URL + 'articles';
      }
      else
      {
        request = SERVER_URL + 'articles/context/' + $scope.searchTerm;
      }
      $scope.gridBusy = $http.get(request)
        .success(function(data) {
          $scope.articlesData = data;
          $scope.articlesData = $scope.articlesData.map(function(article){
            article['date'] = moment(article['date'],"DD-MM-YYYY").toDate();
            return article;
          });
          $scope.gridOptions.data = $scope.articlesData;
          $scope.dateRangeSelected($scope.dateRange); //for the pagination to get refreshed
        });
    };

  });
