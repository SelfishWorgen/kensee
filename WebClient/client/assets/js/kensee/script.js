"use strict";

$(document).ready(function () {
    //if(!window.localStorage.getItem('user')){
    //    window.location.href = 'login.html';
    //}

    var addReport = $('.btn.btn-minier.btn-danger');
    var tabs = $('.tabs li');
    var contents = $('.page-content');
    var logout = $('.logout');
    var reports = {
        rent: [],
        investment: [],
        construction: []
    };

    logout.on('click', function(){
        window.localStorage.removeItem('user');
        window.location.href = 'login.html';
    });

    tabs.on('click', function(){
        var _this = $(this);
        tabs.removeClass('active');
        _this.addClass('active');
        contents.removeClass('active');
        $('.page-content[data-type="' + _this.text().toLowerCase() + '"]').addClass('active');
    });

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
    });

    $("#Area").change(function () {
        $('#Sub').val($("#Area option:selected").data('sub'));
        $('#District').val($("#Area option:selected").data('district'));
    });
    $("#AreaC").change(function () {
        $('#SubC').val($("#AreaC option:selected").data('sub'));
        $('#DistrictC').val($("#AreaC option:selected").data('district'));
    });
    $('.form-inline').submit(function (e) {
        if ($('#Area').val() == "") {
            $('#errorMsg').removeClass('hide');
            return false;
        }
        else {
            $('#errorMsg').addClass('hide');
            return true;
        }
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

    /**********************************************/
    /****** Widget box drag & drop  ***************/
    /**********************************************/
    //$('.widget-container-col.ui-sortable').sortable({
    //    connectWith: '.widget-container-col',
    //    items: '> .widget-box',
    //    handle: ace.vars['touch'] ? '.widget-header' : false,
    //    cancel: '.fullscreen',
    //    opacity: 0.8,
    //    revert: true,
    //    forceHelperSize: true,
    //    placeholder: 'widget-placeholder',
    //    forcePlaceholderSize: true,
    //    tolerance: 'pointer',
    //    start: function (event, ui) {
    //        //when an element is moved, it's parent becomes empty with almost zero height.
    //        //we set a min-height for it to be large enough so that later we can easily drop elements back onto it
    //        ui.item.parent().css({ 'min-height': ui.item.height() })
    //        //ui.sender.css({'min-height':ui.item.height() , 'background-color' : '#F5F5F5'})
    //    },
    //    update: function (event, ui) {
    //        ui.item.parent({ 'min-height': '' })
    //        //p.style.removeProperty('background-color');
    //    }
    //});


});

function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(window.location.search);
    if (results == null)
        return "";
    else
        return decodeURIComponent(results[1].replace(/\+/g, " "));
}
