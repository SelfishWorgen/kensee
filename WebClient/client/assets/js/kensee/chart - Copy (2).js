$(document).ready(function () {
    /**********************************************/
    /****** Get search params  ********************/
    /**********************************************/
    var Area = getParameterByName('Area');
    var Sub = getParameterByName('Sub');
    var District = getParameterByName('District');
    var AreaC = getParameterByName('AreaC');
    var SubC = getParameterByName('SubC');
    var DistrictC = getParameterByName('DistrictC');
    var PropertyType = getParameterByName('PropertyType');
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
    data = new Array();
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
            data: data[Area],
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
            data: data[AreaC],
        };
    }

    if (dataset1 != "" || dataset2 != "") {
        var datasets = new Array();
        if (dataset1 != "")
            datasets.push(dataset1);
        if (dataset2 != "")
            datasets.push(dataset2);

        var RentTakeup = $("#rent-takeup-chart");
        var dataRentTakeup = {
            labels: ["2011-Q1", "2011-Q2", "2011-Q3", "2011-Q4", "2012-Q1", "2012-Q2", "2012-Q3", "2012-Q4", "2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
            datasets: datasets
        };
        var myLineChart = new Chart(RentTakeup, {
            type: 'line',
            data: dataRentTakeup
        });
    }

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
            data: data[Area],
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
            data: data[AreaC],
        };
    }

    if (dataset1 != "" || dataset2 != "") {
        var datasets = new Array();
        if (dataset1 != "")
            datasets.push(dataset1);
        if (dataset2 != "")
            datasets.push(dataset2);

        var RentAvailability = $("#rent-availability-chart");
        var dataRentAvailability = {
            labels: ["2012-Q1", "2012-Q2", "2012-Q3", "2012-Q4", "2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
            datasets: datasets
        };
        var myLineChart = new Chart(RentAvailability, {
            type: 'line',
            data: dataRentAvailability
        });
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
            data: data[Area],
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
            data: data[AreaC],
        };
    }

    if (dataset1 != "" || dataset2 != "") {
        var datasets = new Array();
        if (dataset1 != "")
            datasets.push(dataset1);
        if (dataset2 != "")
            datasets.push(dataset2);

        var RentVacancy = $("#rent-vacancy-chart");
        var dataRentVacancy = {
            labels: ["2006-Q1", "2006-Q2", "2006-Q3", "2006-Q4", "2007-Q1", "2007-Q2", "2007-Q3", "2007-Q4", "2008-Q1", "2008-Q2", "2008-Q3", "2008-Q4", "2009-Q1", "2009-Q2", "2009-Q3", "2009-Q4", "2010-Q1", "2010-Q2", "2010-Q3", "2010-Q4", "2011-Q1", "2011-Q2", "2011-Q3", "2011-Q4", "2012-Q1", "2012-Q2", "2012-Q3", "2012-Q4", "2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
            datasets: datasets
        };
        var myLineChart = new Chart(RentVacancy, {
            type: 'line',
            data: dataRentVacancy
        });
    }

    /****** Rent Prime  ***************************/
    data = new Array();
    data["City"] = [56.04, 56.3, 49.65, 50.75, 51.75, 51.95, 53.85, 55.6, 59.05, 59.95, 61.35, 62.85, 64.6];
    data["West-End"] = [97.19, 96.69, 69.95, 66.69, 68.42, 70.87, 74.92, 76.15, 79.6, 81.47, 83.71, 84.73, 84.92];
    data["Docklands"] = [38, 38, 31.25, 31.25, 31.75, 32.5, 32.5, 33, 34, 34.5, 34.5, 36.5, 38.33];
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
            data: data[Area],
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
            data: data[AreaC],
        };
    }

    if (dataset1 != "" || dataset2 != "") {
        var datasets = new Array();
        if (dataset1 != "")
            datasets.push(dataset1);
        if (dataset2 != "")
            datasets.push(dataset2);

        var RentPrime = $("#rent-prime-chart");
        var dataRentPrime = {
            labels: ["2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
            datasets: datasets
        };
        var myLineChart = new Chart(RentPrime, {
            type: 'line',
            data: dataRentPrime
        });
    }
    /****** Rent Occupiers ***************************/
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
            data: data[Area],
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
            data: data[AreaC],
        };
    }

    if (dataset1 != "" || dataset2 != "") {
        var datasets = new Array();
        if (dataset1 != "")
            datasets.push(dataset1);
        if (dataset2 != "")
            datasets.push(dataset2);

        var InvestmentStock = $("#investment-stock-chart");
        var dataInvestmentStock = {
            labels: ["2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
            datasets: datasets
        };
        var myLineChart = new Chart(InvestmentStock, {
            type: 'line',
            data: dataInvestmentStock
        });
    }
    /****** Investment Absorption ***************************/
    var InvestmentAbsorption = $("#investment-absorption-chart");

    var dataInvestmentAbsorption = {
        labels: ["2005-Q2", "2005-Q4", "2006-Q2", "2006-Q4", "2007-Q2", "2007-Q4", "2008-Q2", "2008-Q4", "2009-Q2", "2009-Q4", "2010-Q2", "2010-Q4", "2011-Q2", "2011-Q4", "2012-Q2", "2012-Q4", "2013-Q2", "2013-Q4", "2014-Q2", "2014-Q4", "2015-Q2", "2015-Q4", "2016-Q1"],
        datasets: [
            {
                label: "City",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "rgba(179,181,198,0.2)",
                borderColor: "rgba(179,181,198,1)",
                pointBackgroundColor: "rgba(179,181,198,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(179,181,198,1)",
                data: [1.06, 1.60, 1.70, 1.72, 1.53, 1.65, 1.66, 1.21, 0.85, 2.09, 2.18, 1.80, 1.58, 1.37, 1.44, 1.02, 1.56, 1.71, 1.77, 2.27, 2.43, 1.75, 1.24],
            },
            {
                label: "West-End",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "rgba(255,99,132,0.2)",
                borderColor: "rgba(255,99,132,1)",
                pointBackgroundColor: "rgba(255,99,132,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,132,1)",
                data: [1.1, 2.05, 1.53, 1.79, 1.61, 1.27, 1.54, 1.27, 0.58, 1.38, 1.23, 1.74, 1.74, 1.79, 0.99, 1.44, 1.27, 1.89, 1.63, 1.85, 1.35, 1.54, 1.36],
            }
        ]
    };
    var myLineChart = new Chart(InvestmentAbsorption, {
        type: 'line',
        data: dataInvestmentAbsorption
    });

    /****** Investment Yields ***************************/
    var InvestmentYields = $("#investment-yields-chart");

    var dataInvestmentYields = {
        labels: ["2006-Q1", "2006-Q2", "2006-Q3", "2006-Q4", "2007-Q1", "2007-Q2", "2007-Q3", "2007-Q4", "2008-Q1", "2008-Q2", "2008-Q3", "2008-Q4", "2009-Q1", "2009-Q2", "2009-Q3", "2009-Q4", "2010-Q1", "2010-Q2", "2010-Q3", "2010-Q4", "2011-Q1", "2011-Q2", "2011-Q3", "2011-Q4", "2012-Q1", "2012-Q2", "2012-Q3", "2012-Q4", "2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
        datasets: [
            {
                label: "City",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "rgba(179,181,198,0.2)",
                borderColor: "rgba(179,181,198,1)",
                pointBackgroundColor: "rgba(179,181,198,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(179,181,198,1)",
                data: [4.57, 4.56, 4.34, 4.34, 4.36, 4.6, 4.8, 4.99, 5.2, 5.4, 5.61, 5.81, 6, 6.19, 6.37, 6.56, 6.76, 6.88, 6.64, 6.42, 6.2, 5.91, 5.83, 5.64, 5.43, 5.42, 5.43, 5.44, 5.45, 5.46, 5.46, 5.24, 5.09, 4.97, 4.77, 4.66, 4.53, 4.5, 4.31, 4.3, 4.31],
            },
            {
                label: "West-End",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "rgba(255,99,132,0.2)",
                borderColor: "rgba(255,99,132,1)",
                pointBackgroundColor: "rgba(255,99,132,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,132,1)",
                data: [3.81, 3.82, 3.79, 3.61, 3.62, 4.04, 4.41, 4.61, 4.64, 5.09, 5.4, 5.64, 5.46, 5.03, 4.64, 4.42, 4.33, 4.17, 4.19, 4.19, 4.2, 4.2, 4.21, 4.22, 2.22, 4.22, 4.23, 4.24, 2.24, 4.12, 4.01, 4.01, 4.01, 4.02, 4.02, 4.03, 3.9, 3.8, 3.8, 3.81, 3.81],
            }
        ]
    };
    var myLineChart = new Chart(InvestmentYields, {
        type: 'line',
        data: dataInvestmentYields
    });

    /****** Investment Sell ***************************/
    var InvestmentSell = $("#investment-sell-chart");

    var dataInvestmentSell = {
        labels: ["2006-Q1", "2006-Q2", "2006-Q3", "2006-Q4", "2007-Q1", "2007-Q2", "2007-Q3", "2007-Q4", "2008-Q1", "2008-Q2", "2008-Q3", "2008-Q4", "2009-Q1", "2009-Q2", "2009-Q3", "2009-Q4", "2010-Q1", "2010-Q2", "2010-Q3", "2010-Q4", "2011-Q1", "2011-Q2", "2011-Q3", "2011-Q4", "2012-Q1", "2012-Q2", "2012-Q3", "2012-Q4", "2013-Q1", "2013-Q2", "2013-Q3", "2013-Q4", "2014-Q1", "2014-Q2", "2014-Q3", "2014-Q4", "2015-Q1", "2015-Q2", "2015-Q3", "2015-Q4", "2016-Q1"],
        datasets: [
            {
                label: "City",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "rgba(179,181,198,0.2)",
                borderColor: "rgba(179,181,198,1)",
                pointBackgroundColor: "rgba(179,181,198,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(179,181,198,1)",
                data: [2518, 3221, 2157, 2071, 2499, 3410, 3116, 970, 1388, 1074, 457, 315, 476, 827, 1768, 675, 580, 1131, 1654, 1768, 1236, 2033, 1293, 1445, 1616, 2603, 1834, 1919, 1150, 1350, 2138, 5224, 989, 2185, 1957, 2983, 1768, 2679, 1758, 2613, 1150],
            },
            {
                label: "West-End",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "rgba(255,99,132,0.2)",
                borderColor: "rgba(255,99,132,1)",
                pointBackgroundColor: "rgba(255,99,132,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(255,99,132,1)",
                data: [968, 712, 1377, 1871, 1358, 1662, 2127, 617, 1063, 589, 551, 322, 380, 589, 1234, 798, 808, 1453, 892, 1215, 1310, 846, 978, 940, 1510, 1301, 1358, 1121, 1624, 1804, 2583, 1662, 1196, 1178, 1634, 1994, 1149, 1653, 1842, 1947, 1453],
            },
            {
                label: "Docklands",
                fill: false,
                lineTension: 0.1,
                backgroundColor: "rgba(132,99,255,0.2)",
                borderColor: "rgba(132,99,255,1)",
                pointBackgroundColor: "rgba(132,99,255,1)",
                pointBorderColor: "#fff",
                pointHoverBackgroundColor: "#fff",
                pointHoverBorderColor: "rgba(132,99,255,1)",
                data: [466, 0, 0, 295, 47, 1216, 1643, 38, 0, 76, 0, 1093, 0, 0, 0, 969, 237, 190, 48, 513, 0, 0, 0, 57, 465, 181, 0, 48, 570, 57, 228, 0, 285, 817, 513, 1178, 0, 66, 228, 246, 646],
            }
        ]
    };
    var myLineChart = new Chart(InvestmentSell, {
        type: 'line',
        data: dataInvestmentSell
    });

    /****** Investment Purchaser ***************************/
    var InvestmentPurchaser = $("#investment-purchaser-chart");

    var dataInvestmentPurchaser = {
        labels: ["UK Institutions", "UK Prop Co", "UK Other", "USA / Canada", "Middle East / North Africa", "Europe", "Asia", "Overseas Other", "Unknown"],
        datasets: [
             {
                 data: [11, 12, 6, 6, 10, 13, 28, 11, 3],
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
             }]
    };
    var myLineChart = new Chart(InvestmentPurchaser, {
        type: 'pie',
        data: dataInvestmentPurchaser
    });


    /****** Constructions Under ***************************/
    var ConstructionsUnder = $("#constructions-under-chart");

    var dataConstructionsUnder = {
        labels: ["2001", "2002", "2003", "2004", "2005", "2006", "2007", "2008", "2009", "2010", "2011", "2012", "2013", "2014", "2015", "2016", "2017", "2018", "2019"],
        datasets: [
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
                data: [1.11, 1.11, 1.11, 2.7, 1.01, 0.81, 0.7, 2, 1.2, 2.21, 0.63, 1.01, 1.42, 1.82, 1.43, 0.32, 0, 0, 0],
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
                data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.3, 0.52, 0, 0],
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
                data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.61, 1.43, 0.71],
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
                data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5, 0.69, 0.19, 0],
            }
        ]
    };
    var myLineChart = new Chart(ConstructionsUnder, {
        type: 'bar',
        data: dataConstructionsUnder
    });




});
