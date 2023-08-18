$(function () {
    
    var mainhub = $.connection.myHub;
    var cpux = [];
    var cpuy = [];
    var memox = [];
    var memoy = [];
    var chart = new Chart(document.getElementById('chart').getContext('2d'), {
        type: 'line',
        data: [],
        options: []
    });
    $("#chart").hide();
    let a = function () { };
    var graph = 0;
    mainhub.client.printPercent = function (gpu, memo , x, y) {
        var cpuValue = $('#cpu');
        var memoValue = $('#memo');
        let l = Math.max(x["X"].length - 30, 0);
        cpux = x["X"].slice(l, x["X"].length);
        cpuy = x["Y"].slice(l, x["X"].length);
        memox = y["X"].slice(l, x["X"].length);
        memoy = y["Y"].slice(l, x["X"].length);
        if (graph == 1) addDataToChart(cpux, cpuy);
        if (graph == -1) addDataToChart(memox, memoy);
        cpuValue.text(gpu + ' %');
        memoValue.text(memo + ' MBytes');
    }
    mainhub.client.updatepoints = function (dicts) {
        drawChart(dicts["X"], dicts["Y"]);

    }
    mainhub.client.begindraw = function () {
        a = doSomething(1);

    }
    mainhub.client.showloadings = function (loadings, type) {
        var tablehead = $("#users-table thead");
        tablehead.empty();
        var row = $("<tr>");
        row.append($("<th>").text("Номер"));
        row.append($("<th>").text("Время"));
       
        tablehead.append(row);
        var tableBody = $("#users-table tbody");
        tableBody.empty();
        for (let i = 0; i < (loadings["Id"].length -1); i++) {
            var row = $("<tr>");
            row.append($("<td>").text(loadings["Number"][i]));
            row.append($("<td>").text(loadings["Seconds"][i]));
            
            row.on('click', function () {
                $('#users-table tbody tr').removeClass('red');
                $('#users-table tbody tr').addClass('white');
                $(this).removeClass('white');
                $(this).addClass('red');
              
                mainhub.server.requestGraphPoints(loadings["Id"][i], type, 0, loadings["Seconds"][i]);
                $("#leftborder").attr('value', 0);
                $("#rightborder").attr('value', loadings["Seconds"][i]);
                $('#periodbutton').click(function () {
                    
                    var left = $("#leftborder").val();
                    var right = $("#rightborder").val();
                    if (left >= right) 
                        $("#error2").text("Левая граница должна быть меньше правой");
                    else {
                        $("#error2").text("");
                        mainhub.server.requestGraphPoints(loadings["Id"][i], type, left, right);
                        $("#chart").show();
                      
                    }
                    
                });
                $(".bordersBlock").show();
            });
            tableBody.append(row);
            
        }
        $("#users-table").show();
    }
    
    function drawChart(labels, data) {
        
        var chartData = {
            labels: labels,
            datasets: [{
                label: 'График изменения процента загрузок',
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1,
                data: data
            }]
        };

        var chartOptions = {
            responsive: true,
            scales: {
                x: {
                    title: {
                        display: true,
                        text: 'Seconds'
                    }
                },
                y: {
                    title: {
                        display: true,
                        text: 'Percent'
                    }
                }
            }
        };

        chart = new Chart(document.getElementById('chart').getContext('2d'), {
            type: 'line',
            data: chartData,
            options: chartOptions
        });
    }
    function addDataToChart(x , y) {
        chart.data.labels = x;
        chart.data.datasets[0].data = y;
        chart.update();
    }
    function doSomething(timer) {
        return setInterval(() => {
            mainhub.server.printvalues();
        }, timer * 1000);
    }
    
    $(document).ready(function () {
      
        $('#timeline').each(function () {
            $(this).data('previousValue', $(this).val());
        });

        $('#timeline').on('input', function () {
            var previousValue = $(this).data('previousValue');
            var currentValue = $(this).val();

            if (currentValue >= 1) {
                $(this).data('previousValue', currentValue);
                $('#error').text("");
                clearInterval(a);
                a = doSomething(currentValue);
            }
            else {
                $('#timeline').val(previousValue);
                $('#error').text("Ошибка: значение не может быть меньше 1");
            }
        });
        
    });
    $.connection.hub.start().done(function () {
        $('.cpu-button').click(function () {
            graph = 1;
            $(".bordersBlock").hide();
            $("#users-table").hide();
           
            drawChart(cpux, cpuy);
            $("#chart").show();
            
        });

        $('.memo-button').click(function () {
            graph = -1;
           
            $(".bordersBlock").hide();

            drawChart(memox, memoy);
            $("#users-table").hide();
            $("#chart").show();
        });

        $('.cpu-oldbutton').click(function () {
            graph = 0;
           
            $(".bordersBlock").hide();
          
            $("#users-table").show();
            mainhub.server.getMainLoadings("CPU");
            $("#chart").hide();
        });
        $('.memo-oldbutton').click(function () {

            graph = 0;
            $(".bordersBlock").hide();

            $("#users-table").show();
            mainhub.server.getMainLoadings("Memory");
            $("#chart").hide();
        });
        mainhub.server.upload();       

    });

});
