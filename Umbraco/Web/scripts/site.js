//function myelem(value, options) {
//    var el = document.createElement('select');
//    el.name = 'ExerciseName';
//    var category = $('#categorySelected').val();
//    $.get('/umbraco/surface/DataTypeSurface/GetExerciseByCategory', { id: category }, function (data) {
//        $(el).append(data);
//        $(el).find("option").filter(function (index) {
//            return value === $(this).text();
//        }).prop("selected", "selected");
//    });
//    return el;
//}

//function myvalue(elem, operation, value) {
//    if (operation === 'get') {
//        return $(elem).val();
//    } else if (operation === 'set') {
//        //$('input', elem).val(value);
//        $(el).find("option").filter(function (index) {
//            return value === $(this).text();
//        }).prop("selected", "selected");
//    }
//}

//function myelem2(value, options) {
//    var el = document.createElement('select');
//    el.name = 'Device';
//    var platform = $('#platformSelected').val();
//    $.get('/umbraco/surface/DataTypeSurface/GetDeviceByPlatform', { id: platform }, function (data) {
//        $(el).append(data);
//        $(el).find("option").filter(function (index) {
//            return value === $(this).text();
//        }).prop("selected", "selected");
//    });
//    return el;
//}

//function myvalue2(elem, operation, value) {
//    var a = 1;
//    var b = 2;
//    var c = a + b;
//    if (operation === 'get') {
//        return $(elem).val();
//    } else if (operation === 'set') {
//        //$('input', elem).val(value);
//        $(el).find("option").filter(function (index) {
//            return value === $(this).text();
//        }).prop("selected", "selected");
//    }
//    var d = 5;
//    var e = 6;
//    var f = d + e;
//}

$(document).ready(function () {
    if ($('#pnlForm').length) {
//        var lastSel;
//        $('#grid').jqGrid({
//            datatype: function () {
//                var pageSize = $('#grid').getGridParam('rowNum');
//                var currentPage = $('#grid').getGridParam('page');
//                var sortColumn = $('#grid').getGridParam('sortname');
//                var sortOrder = $('#grid').getGridParam('sortorder');
//                var isSearch = $('#grid').getGridParam('search');
//                var searchOptions;
//                var searchField = '';
//                var searchOper = '';
//                var searchString = '';
//                var filters = '';
//                if (isSearch == true) { //Obtiene los parámetros de busqueda en caso de que la variable _search sea true
//                    searchOptions = $('#grid').getGridParam('postData');
//                    searchField = searchOptions.searchField;
//                    searchOper = searchOptions.searchOper;
//                    searchString = searchOptions.searchString;
//                    filters = searchOptions.filters;
//                }
//                var grid = {
//                    //Parametros de entrada
//                    PageSize: pageSize,
//                    CurrentPage: currentPage,
//                    SortColumn: sortColumn,
//                    SortOrder: sortOrder,
//                    IsSearch: isSearch,
//                    SearchField: searchField,
//                    SearchValue: searchString,
//                    SearchOper: searchOper,
//                    Filters: filters
//                };

//                var json = JSON.stringify(grid);
//                $.ajax({
//                    url: '/umbraco/surface/DataTypeSurface/GetExercise',
//                    cache: false,
//                    data: json,
//                    dataType: 'json',
//                    type: 'POST',
//                    contentType: 'application/json; charset=utf-8',
//                    success: function (data, state) { //success: function (data, textStatus, jqXHR) {
//                        if (state == 'success') {
//                            $('#grid')[0].addJSONData(data);
//                        }
//                    },
//                    error: function (xhr, ajaxOptions, thrownError) { //error: function (jqXHR, textStatus, errorThrown) {
//                        alert('Error: ' + xhr.status + ' ' + xhr.statusText);
//                    }
//                });
//            },
//            jsonReader: {
//                root: 'root',
//                page: 'page',
//                total: 'total',
//                records: 'records',
//                repeatitems: true,
//                cell: 'cell',
//                id: 'id'
//            },
//            colModel: [//Columns
//                {name: 'Id', index: 'Id', width: 80, align: 'center', label: 'Id', hidden: false, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, edittype: 'text', formatter: 'integer', editoptions: { readonly: true, size: 10} },
//                { name: 'Category', index: 'Category', width: 200, align: 'left', label: 'Category', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: true, edittype: 'select', editoptions: { dataUrl: '/umbraco/surface/DataTypeSurface/GetPrevalue?id=1085'} },
//                { name: 'ExerciseName', index: 'ExerciseName', width: 250, align: 'center', label: 'Exercise', hidden: false, sortable: true, search: false, editable: true, edittype: 'text', editrules: { edithidden: true, required: true }, editoptions: { size: 40, maxlength: 45} },
//                { name: 'IsActive', index: 'IsActive', width: 150, align: 'center', label: 'IsActive', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: true, edittype: "checkbox", editoptions: { value: "True:False"} },
//                { name: 'CategoryId', index: 'CategoryId', width: 80, align: 'center', label: 'CategoryId', hidden: true, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, editable: false }

//            ],
//            //            onSelectRow: function (id) {
//            //                if (id && id !== lastSel) {
//            //                    $('#grid').restoreRow(lastSel);
//            //                    lastSel = id;
//            //                }

//            //                $('#grid').jqGrid('saveRow', id,
//            //                    {
//            //                        successfunc: function (result) {
//            //                            var a = 1;
//            //                            var b = 2;
//            //                            var c = a + b;
//            //                            alert('saveRow - success - ' + result.responseText);
//            //                            return true;
//            //                        },
//            //                        url: '/umbraco/surface/DataTypeSurface/SaveRow',
//            //                        extraparam: { oper: 'save' },
//            //                        aftersavefunc: function (rowid, result) {
//            //                            var a = 1;
//            //                            var b = 2;
//            //                            var c = a + b;
//            //                            alert('saveRow - saved - ' + rowid + ' - ' + result.responseText);
//            //                        },
//            //                        errorfunc: function (rowid, result) {
//            //                            var a = 1;
//            //                            var b = 2;
//            //                            var c = a + b;
//            //                            alert('saveRow - error - ' + rowid + ' - ' + result.responseText);
//            //                        },
//            //                        afterrestorefunc: function (rowid) {
//            //                            var a = 1;
//            //                            var b = 2;
//            //                            var c = a + b;
//            //                            alert('saveRow - after restore - ' + rowid);
//            //                        },
//            //                        restoreAfterError: true,
//            //                        mtype: 'POST'
//            //                    }
//            //                );

//            //                $('#grid').jqGrid('editRow', id,
//            //                    {
//            //                        keys: false,
//            //                        oneditfunc: function (rowid) {
//            //                            var a = 1;
//            //                            var b = 2;
//            //                            var c = a + b;
//            //                            alert('editRow - onedit - ' + rowid);
//            //                        } ,
//            //                                                successfunc: function (result) {
//            //                                                    var a = 1;
//            //                                                    var b = 2;
//            //                                                    var c = a + b;
//            //                                                    alert('editRow - edited - ' + result.responseText);
//            //                                                    return true;
//            //                                                },
//            //                                                url: '/umbraco/surface/DataTypeSurface/EditRow',
//            //                                                extraparam: { oper: 'edit' },
//            //                                                aftersavefunc: function (rowid, result) {
//            //                                                    var a = 1;
//            //                                                    var b = 2;
//            //                                                    var c = a + b;
//            //                                                    alert('editRow - saved - ' + rowid + ' - ' + result.responseText);
//            //                                                },
//            //                                                errorfunc: function (rowid, result) {
//            //                                                    var a = 1;
//            //                                                    var b = 2;
//            //                                                    var c = a + b;
//            //                                                    alert('editRow - error - ' + rowid + ' - ' + result.responseText);
//            //                                                },
//            //                                                afterrestorefunc: function (rowid) {
//            //                                                    var a = 1;
//            //                                                    var b = 2;
//            //                                                    var c = a + b;
//            //                                                    alert('editRow - after restore - ' + rowid);
//            //                                                },
//            //                                                restoreAfterError: true,
//            //                                                mtype: 'POST'
//            //                    });
//            //            },
//            editurl: '/umbraco/surface/DataTypeSurface/SetExercise',
//            pager: '#pager', //Pager.
//            loadtext: 'Cargando datos...',
//            recordtext: '{0} - {1} de {2} elementos',
//            emptyrecords: 'No hay resultados',
//            pgtext: 'Pág: {0} de {1}', //Paging input control text format.
//            rowNum: '10', // PageSize.
//            rowList: [10, 20, 30, 40], //Variable PageSize DropDownList. 
//            viewrecords: true, //Show the RecordCount in the pager.
//            multiselect: false,
//            sortname: 'Id', //Default SortColumn
//            sortorder: 'asc', //Default SortOrder.
//            width: '850',
//            height: '400',
//            imgpath: '/css/ui/images',
//            caption: 'Exercise'
//        }).navGrid('#pager',
//                { edit: false, add: false, del: false, search: false, view: false },
//                {}, //Options for the Edit Dialog
//                {}, //Options for the Add Dialog
//                {}, //Options for Delete
//                {}, //Options for Search
//                {}  //Options for View
//        ).inlineNav('#pager',
//            {
//                addParams: { useFormatter: false },
//                editParams: {
//                    aftersavefunc: function (rowid, result) {
//                        $('#grid').trigger('reloadGrid');
//                    }
//                }
//            });
    }


    if ($('#pnlForm1').length) {
        $.get('/umbraco/surface/DataTypeSurface/GetPartial', { name: '_Routine' }, function (data) {
            $('#pnlForm1').empty();
            $('#pnlForm1').html(data);
        });

        $('#body_prop_test_Insertexercise').parent().empty();

        //                $('#addWorkout').click(function (e) {
        //                    var items = $('#grid1').jqGrid('getDataIDs');
        //                    var name = $('#templateName').val();
        //                    if (name != "" && items.length > 0) {
        //                        $.post('/umbraco/surface/DataTypeSurface/InsertTemplateWorkout', { name: name }, function (data) {
        //                            var a = 1;
        //                            var b = 2;
        //                            var c = a + b;
        //                            $('#templateName').val('');
        //                            var d = 5;
        //                            var g = 6;
        //                            var f = d + e;
        //                        });
        //                    }
        //                    e.preventDefault();
        //                });

        //                var lastSel;
        //                $('#grid1').jqGrid({
        //                    datatype: function () {
        //                        var pageSize = $('#grid1').getGridParam('rowNum');
        //                        var currentPage = $('#grid1').getGridParam('page');
        //                        var sortColumn = $('#grid1').getGridParam('sortname');
        //                        var sortOrder = $('#grid1').getGridParam('sortorder');
        //                        var isSearch = $('#grid1').getGridParam('search');
        //                        var searchOptions;
        //                        var searchField = '';
        //                        var searchOper = '';
        //                        var searchString = '';
        //                        var filters = '';
        //                        if (isSearch == true) { //Obtiene los parámetros de busqueda en caso de que la variable _search sea true
        //                            searchOptions = $('#grid1').getGridParam('postData');
        //                            searchField = searchOptions.searchField;
        //                            searchOper = searchOptions.searchOper;
        //                            searchString = searchOptions.searchString;
        //                            filters = searchOptions.filters;
        //                        }
        //                        var grid = {
        //                            //Parametros de entrada
        //                            PageSize: pageSize,
        //                            CurrentPage: currentPage,
        //                            SortColumn: sortColumn,
        //                            SortOrder: sortOrder,
        //                            IsSearch: isSearch,
        //                            SearchField: searchField,
        //                            SearchValue: searchString,
        //                            SearchOper: searchOper,
        //                            Filters: filters
        //                        };

        //                        var json = JSON.stringify(grid);
        //                        $.ajax({
        //                            url: '/umbraco/surface/DataTypeSurface/GetRoutine',
        //                            cache: false,
        //                            data: json,
        //                            dataType: 'json',
        //                            type: 'POST',
        //                            contentType: 'application/json; charset=utf-8',
        //                            success: function (data, state) { //success: function (data, textStatus, jqXHR) {
        //                                if (state == 'success') {
        //                                    $('#grid1')[0].addJSONData(data);
        //                                }
        //                            },
        //                            error: function (xhr, ajaxOptions, thrownError) { //error: function (jqXHR, textStatus, errorThrown) {
        //                                alert('Error: ' + xhr.status + ' ' + xhr.statusText);
        //                            }
        //                        });
        //                    },
        //                    jsonReader: {
        //                        root: 'root',
        //                        page: 'page',
        //                        total: 'total',
        //                        records: 'records',
        //                        repeatitems: true,
        //                        cell: 'cell',
        //                        id: 'id'
        //                    },
        //                    colModel: [//Columns
        //                        {name: 'Id', index: 'Id', width: 80, align: 'center', label: 'Id', hidden: false, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, edittype: 'text', formatter: 'integer', editoptions: { readonly: true, size: 10} },
        //                        { name: 'Category', index: 'Category', width: 250, align: 'left', label: 'Category', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: true, edittype: 'select', editoptions: { dataUrl: '/umbraco/surface/DataTypeSurface/GetPrevalue?id=1085', dataEvents: [{ type: 'change', fn: function (e) {
        //                            var idSelected = this.value;
        //                            //alert(idSelected);
        //                            //$('input[name="Value"]').val(this.value);
        //                            $.get('/umbraco/surface/DataTypeSurface/GetExerciseByCategory', { id: idSelected }, function (data) {
        //                                $('select[name="ExerciseName"]').empty();
        //                                $('select[name="ExerciseName"]').append(data);
        //                            });

        //                        }
        //                        }]
        //                        }
        //                        },
        //                        { name: 'ExerciseName', index: 'ExerciseName', width: 320, align: 'center', label: 'Exercise', hidden: false, sortable: true, search: false, editable: true, edittype: 'custom', editoptions: { custom_element: myelem, custom_value: myvalue} },
        //                        { name: 'Type', index: 'Type', width: 100, align: 'left', label: 'Type', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: false },
        //                        { name: 'Unit', index: 'Unit', width: 100, align: 'left', label: 'Unit', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: false },
        //                        { name: 'Value', index: 'Value', width: 100, align: 'left', label: 'Value', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: true, edittype: 'text', editrules: { edithidden: true, required: true }, editoptions: { size: 40, maxlength: 45} },
        //                        { name: 'State', index: 'State', width: 100, align: 'left', label: 'State', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: false },
        //                    //{ name: 'Description', index: 'Description', width: 150, align: 'center', label: 'Description', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: true, edittype: 'textarea', editrules: { edithidden: true, required: true }, editoptions: { rows: '10', cols: '30'} },
        //                    //{ name: 'IsActive', index: 'IsActive', width: 150, align: 'center', label: 'IsActive', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: true, edittype: "checkbox", editoptions: { value: "True:False"} },
        //                    {name: 'CategoryId', index: 'CategoryId', width: 80, align: 'center', label: 'CategoryId', hidden: true, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, editable: false },
        //                        { name: 'ExerciseId', index: 'ExerciseId', width: 80, align: 'center', label: 'ExerciseId', hidden: true, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, editable: false }
        //                    //{ name: 'SortOrder', index: 'SortOrder', width: 80, align: 'center', label: 'SortOrder', hidden: false, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, editable: false }
        //                    //{ name: 'UnitId', index: 'UnitId', width: 80, align: 'center', label: 'UnitId', hidden: true, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, editable: false }
        //                    ],
        //                    onSelectRow: function (id) {
        //                        if (id != "new_row") {
        //                            $('#categorySelected').val($("#grid1").jqGrid('getCell', id, 'CategoryId'));
        //                        }
        //                    },
        //                    editurl: '/umbraco/surface/DataTypeSurface/SetRoutine',
        //                    pager: '#pager1', //Pager.
        //                    loadtext: 'Cargando datos...',
        //                    recordtext: '{0} - {1} de {2} elementos',
        //                    emptyrecords: 'No hay resultados',
        //                    pgtext: 'Pág: {0} de {1}', //Paging input control text format.
        //                    rowNum: '10', // PageSize.
        //                    rowList: [10, 20, 30, 40], //Variable PageSize DropDownList. 
        //                    viewrecords: true, //Show the RecordCount in the pager.
        //                    multiselect: false,
        //                    sortname: 'Id', //Default SortColumn
        //                    sortorder: 'asc', //Default SortOrder.
        //                    width: '1200',
        //                    height: '400',
        //                    imgpath: '/css/ui/images',
        //                    caption: 'Exercise'
        //                }).navGrid('#pager1',
        //                        { edit: false, add: false, del: false, search: false, view: false },
        //                        {}, //Options for the Edit Dialog
        //                        {}, //Options for the Add Dialog
        //                        {}, //Options for Delete
        //                        {}, //Options for Search
        //                        {}  //Options for View
        //                ).inlineNav('#pager1',
        //                    {
        //                        addParams: { useFormatter: false },
        //                        editParams: {
        //                            aftersavefunc: function (rowid, result) {
        //                                $('#grid1').trigger('reloadGrid');
        //                            }
        //                        }
        //                    });

        //                $("#grid1").jqGrid('sortableRows',
        //                    {
        //                        cursor: 'move',
        //                        update: function (e, ui) {
        //                            var routines = new Array();
        //                            var serial = jQuery('#grid1').jqGrid('getRowData');
        //                            $.each(serial, function (index, value) {
        //                                routines.push(value.Id);
        //                            });
        //                            $.post('/umbraco/surface/DataTypeSurface/UpdateOrder', $.param({ routines: routines }, true), function (data) {
        //                                alert(data);
        //                            });
        //                            //alert("item with id=" + ui.item[0].id + " is droped");
        //                            //alert(serial);
        //                            //serialize the array here
        //                        }
        //                    });
    }

    if ($('#pnlForm2').length) {
        var url = '/umbraco/surface/DataTypeSurface/GetMeasurement';
        $.get(url, function (data) {
            $('#pnlForm2').empty();
            $('#pnlForm2').html(data);
            //$('#form1').attr('action', '/umbraco/surface/DataTypeSurface/InsertExercise');

            // $('#carousel').cycle({
            //     fx: 'scrollHorz',
            //     speed: 'fast',
            //     timeout: 0,
            //     next: '#next',
            //     prev: '#prev',
            //     before: function () {

            //     },
            //     after: function () {
            //         $('#dataInfo').html('<h3>' + $(this).attr('data-title') + '</h3><br>').append('<div>' + $(this).attr('data-description') + '</div>');
            //     }
            // });
            $.fn.cycle.updateActivePagerLink = function (pager, currSlideIndex, clsName) {
                $(pager).find('li').removeClass('activeLI').filter('li:eq(' + currSlideIndex + ')').addClass('activeLI');
                //$(pager).children().removeClass('activeLI').filter(':eq(' + currSlideIndex + ')').addClass('activeLI');
                //var title = $(pager).find('li').filter(':eq(' + currSlideIndex + ')').find('img').attr('data-title');
                //var description = $(pager).find('li').filter(':eq(' + currSlideIndex + ')').find('img').attr('data-description');
                //$('#dataInfo').html('<h3>' + title + '</h3><br>').append('<div>' + description + '</div>');
            };

            $('#slideshow').cycle({
                fx: 'fade',
                sync: true,
                speed: 1000,
                timeout: 4000,
                pager: '#nav',
                pagerAnchorBuilder: function (idx, slide) {
                    // return sel string for existing anchor
                    var a = 1;
                    var b = 2;
                    var c = a + b;

                    var d = 5;
                    var e = 6;
                    var f = d + e;

                    var j = 4;
                    var k = 5;
                    var l = j + k;

                    var g = 2;
                    var h = 3;
                    var i = g + h;
                    return '#nav li:eq(' + (idx) + ') a';
                }
            });
        });
    }

    if ($('#pnlForm3').length) {
//        var lastSel;
//        $('#grid3').jqGrid({
//            datatype: function () {
//                var pageSize = $('#grid3').getGridParam('rowNum');
//                var currentPage = $('#grid3').getGridParam('page');
//                var sortColumn = $('#grid3').getGridParam('sortname');
//                var sortOrder = $('#grid3').getGridParam('sortorder');
//                var isSearch = $('#grid3').getGridParam('search');
//                var searchOptions;
//                var searchField = '';
//                var searchOper = '';
//                var searchString = '';
//                var filters = '';
//                if (isSearch == true) { //Obtiene los parámetros de busqueda en caso de que la variable _search sea true
//                    searchOptions = $('#grid3').getGridParam('postData');
//                    searchField = searchOptions.searchField;
//                    searchOper = searchOptions.searchOper;
//                    searchString = searchOptions.searchString;
//                    filters = searchOptions.filters;
//                }
//                var grid = {
//                    //Parametros de entrada
//                    PageSize: pageSize,
//                    CurrentPage: currentPage,
//                    SortColumn: sortColumn,
//                    SortOrder: sortOrder,
//                    IsSearch: isSearch,
//                    SearchField: searchField,
//                    SearchValue: searchString,
//                    SearchOper: searchOper,
//                    Filters: filters
//                };

//                var json = JSON.stringify(grid);
//                $.ajax({
//                    url: '/umbraco/surface/DataTypeSurface/GetDevice',
//                    cache: false,
//                    data: json,
//                    dataType: 'json',
//                    type: 'POST',
//                    contentType: 'application/json; charset=utf-8',
//                    success: function (data, state) { //success: function (data, textStatus, jqXHR) {
//                        if (state == 'success') {
//                            $('#grid3')[0].addJSONData(data);
//                        }
//                    },
//                    error: function (xhr, ajaxOptions, thrownError) { //error: function (jqXHR, textStatus, errorThrown) {
//                        alert('Error: ' + xhr.status + ' ' + xhr.statusText);
//                    }
//                });
//            },
//            jsonReader: {
//                root: 'root',
//                page: 'page',
//                total: 'total',
//                records: 'records',
//                repeatitems: true,
//                cell: 'cell',
//                id: 'id'
//            },
//            colModel: [//Columns
//                {name: 'Id', index: 'Id', width: 80, align: 'center', label: 'Id', hidden: false, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, edittype: 'text', formatter: 'integer', editoptions: { readonly: true, size: 10} },
//                { name: 'Platform', index: 'Platform', width: 100, align: 'left', label: 'Platform', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: true, edittype: 'select', editoptions: { dataUrl: '/umbraco/surface/DataTypeSurface/GetPrevalue?id=1065'} },
//                { name: 'DeviceName', index: 'DeviceName', width: 150, align: 'center', label: 'Device', hidden: false, sortable: true, search: false, editable: true, edittype: 'text', editrules: { edithidden: true, required: true }, editoptions: { size: 40, maxlength: 45} },
//                { name: 'IsActive', index: 'IsActive', width: 150, align: 'center', label: 'IsActive', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: true, edittype: "checkbox", editoptions: { value: "True:False"} },
//                { name: 'PlatformId', index: 'PlatformId', width: 80, align: 'center', label: 'PlatformId', hidden: true, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, editable: false }
//            ],
//            onSelectRow: function (id) {
//                if (id != "new_row") {
//                    $('#categorySelected').val($("#grid3").jqGrid('getCell', id, 'CategoryId'));
//                }
//            },
//            editurl: '/umbraco/surface/DataTypeSurface/SetDevice',
//            pager: '#pager3', //Pager.
//            loadtext: 'Cargando datos...',
//            recordtext: '{0} - {1} de {2} elementos',
//            emptyrecords: 'No hay resultados',
//            pgtext: 'Pág: {0} de {1}', //Paging input control text format.
//            rowNum: '10', // PageSize.
//            rowList: [10, 20, 30, 40], //Variable PageSize DropDownList. 
//            viewrecords: true, //Show the RecordCount in the pager.
//            multiselect: false,
//            sortname: 'Id', //Default SortColumn
//            sortorder: 'asc', //Default SortOrder.
//            width: '800',
//            height: '400',
//            imgpath: '/css/ui/images',
//            caption: 'Device'
//        }).navGrid('#pager3',
//                { edit: false, add: false, del: false, search: false, view: false },
//                {}, //Options for the Edit Dialog
//                {}, //Options for the Add Dialog
//                {}, //Options for Delete
//                {}, //Options for Search
//                {}  //Options for View
//        ).inlineNav('#pager3',
//            {
//                addParams: { useFormatter: false },
//                editParams: {
//                    aftersavefunc: function (rowid, result) {
//                        $('#grid3').trigger('reloadGrid');
//                    }
//                }
//            });
    }

    if ($('#pnlForm4').length) {
//        var lastSel;
//        $('#grid4').jqGrid({
//            datatype: function () {
//                var pageSize = $('#grid4').getGridParam('rowNum');
//                var currentPage = $('#grid4').getGridParam('page');
//                var sortColumn = $('#grid4').getGridParam('sortname');
//                var sortOrder = $('#grid4').getGridParam('sortorder');
//                var isSearch = $('#grid4').getGridParam('search');
//                var searchOptions;
//                var searchField = '';
//                var searchOper = '';
//                var searchString = '';
//                var filters = '';
//                if (isSearch == true) { //Obtiene los parámetros de busqueda en caso de que la variable _search sea true
//                    searchOptions = $('#grid4').getGridParam('postData');
//                    searchField = searchOptions.searchField;
//                    searchOper = searchOptions.searchOper;
//                    searchString = searchOptions.searchString;
//                    filters = searchOptions.filters;
//                }
//                var grid = {
//                    //Parametros de entrada
//                    PageSize: pageSize,
//                    CurrentPage: currentPage,
//                    SortColumn: sortColumn,
//                    SortOrder: sortOrder,
//                    IsSearch: isSearch,
//                    SearchField: searchField,
//                    SearchValue: searchString,
//                    SearchOper: searchOper,
//                    Filters: filters
//                };

//                var json = JSON.stringify(grid);
//                $.ajax({
//                    url: '/umbraco/surface/DataTypeSurface/GetNotification',
//                    cache: false,
//                    data: json,
//                    dataType: 'json',
//                    type: 'POST',
//                    contentType: 'application/json; charset=utf-8',
//                    success: function (data, state) { //success: function (data, textStatus, jqXHR) {
//                        if (state == 'success') {
//                            $('#grid4')[0].addJSONData(data);
//                        }
//                    },
//                    error: function (xhr, ajaxOptions, thrownError) { //error: function (jqXHR, textStatus, errorThrown) {
//                        alert('Error: ' + xhr.status + ' ' + xhr.statusText);
//                    }
//                });
//            },
//            jsonReader: {
//                root: 'root',
//                page: 'page',
//                total: 'total',
//                records: 'records',
//                repeatitems: true,
//                cell: 'cell',
//                id: 'id'
//            },
//            colModel: [//Columns
//                {name: 'Id', index: 'Id', width: 50, align: 'center', label: 'Id', hidden: false, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, edittype: 'text', formatter: 'integer', editoptions: { readonly: true, size: 10} },
//                { name: 'Platform', index: 'Platform', width: 100, align: 'left', label: 'Platform', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: true, edittype: 'select', editoptions: { dataUrl: '/umbraco/surface/DataTypeSurface/GetPrevalue?id=1065', dataEvents: [{ type: 'change', fn: function (e) {
//                    var idSelected = this.value;
//                    $.get('/umbraco/surface/DataTypeSurface/GetDeviceByPlatform', { id: idSelected }, function (data) {
//                        $('select[name="Device"]').empty();
//                        $('select[name="Device"]').append(data);
//                    });
//                }
//                }]
//                }
//                },
//                { name: 'Device', index: 'Device', width: 100, align: 'center', label: 'Device', hidden: false, sortable: true, search: false, editable: true, edittype: 'custom', editoptions: { custom_element: myelem2, custom_value: myvalue2} },
//                { name: 'Token', index: 'Token', width: 250, align: 'left', label: 'Token', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: true, edittype: 'text', editrules: { edithidden: true, required: true }, editoptions: { size: 40, maxlength: 45} },
//                { name: 'IsActive', index: 'IsActive', width: 150, align: 'center', label: 'IsActive', hidden: false, sortable: true, search: true, stype: 'text', searchoptions: { sopt: ['eq', 'bw', 'bn', 'ew', 'en', 'cn', 'nc'] }, editable: true, edittype: "checkbox", editoptions: { value: "True:False"} },
//                { name: 'MemberId', index: 'MemberId', width: 80, align: 'center', label: 'MemberId', hidden: true, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, editable: false },
//                { name: 'PlatformId', index: 'PlatformId', width: 80, align: 'center', label: 'PlatformId', hidden: true, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, editable: false },
//                { name: 'DeviceId', index: 'DeviceId', width: 80, align: 'center', label: 'DeviceId', hidden: true, sortable: true, search: true, stype: 'text', searchrules: { integer: true }, searchoptions: { sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge'] }, editable: false }
//            ],
//            onSelectRow: function (id) {
//                if (id != "new_row") {
//                    $('#platformSelected').val($("#grid4").jqGrid('getCell', id, 'PlatformId'));
//                }
//            },
//            editurl: '/umbraco/surface/DataTypeSurface/SetNotification',
//            pager: '#pager4', //Pager.
//            loadtext: 'Cargando datos...',
//            recordtext: '{0} - {1} de {2} elementos',
//            emptyrecords: 'No hay resultados',
//            pgtext: 'Pág: {0} de {1}', //Paging input control text format.
//            rowNum: '10', // PageSize.
//            rowList: [10, 20, 30, 40], //Variable PageSize DropDownList. 
//            viewrecords: true, //Show the RecordCount in the pager.
//            multiselect: false,
//            sortname: 'Id', //Default SortColumn
//            sortorder: 'asc', //Default SortOrder.
//            width: '800',
//            height: '400',
//            imgpath: '/css/ui/images',
//            caption: 'Notification'
//        }).navGrid('#pager4',
//                { edit: false, add: false, del: false, search: false, view: false },
//                {}, //Options for the Edit Dialog
//                {}, //Options for the Add Dialog
//                {}, //Options for Delete
//                {}, //Options for Search
//                {}  //Options for View
//        ).inlineNav('#pager4',
//            {
//                addParams: { useFormatter: false },
//                editParams: {
//                    aftersavefunc: function (rowid, result) {
//                        $('#grid4').trigger('reloadGrid');
//                    }
//                }
//            });

    }
});
