$("#btnSearch").click(function () {
    $.ajax({
        url: "/Memo/SearchSDGWeigh",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            supplierCode: $("#Supplier").val(),
            millCode: $("#MillCode").val(),
            dateFrom: $("#DateFrom").val(),
            dateTo: $("#DateTo").val()
        }),
        success: function (data) {
            console.table(data);
            $("#result").empty();

            let currentMemo = $("#MemoNo").val();

            $.each(data, function (i, x) {

                let checked = "";
                let disabled = "";

                if (x.memoNo != null && x.memoNo != "") {
                    if (x.memoNo == currentMemo) {
                        checked = "checked";
                    }
                    else {
                        checked = "checked";
                        disabled = "disabled";
                    }
                }

                $("#result").append(`
<tr>
    <td>
        <input type="checkbox"
               class="chkTicket"
               value="${x.serialNo}"
               ${checked}
               ${disabled}>
    </td>
    <td>${x.lorryNo}</td>
    <td>${x.driverCode}</td>
    <td>${x.deliverY_ORDER_NO}</td>
    <td>${x.serialNo}</td>
    <td>${formatDate(x.postDate)}</td>
    <td>${x.weightIn}</td>
    <td>${x.weightOut}</td>
    <td>${x.nettWeight}</td>
    <td>${x.deD_WT}</td>
    <td>${x.memoNo ?? ""}</td>
</tr>
`);
            });
        },
        error: function (xhr) {
            alert(xhr.responseText);
        }
    });
});


$("#chkAll").click(function () {
    $(".chkTicket:not(:disabled)")
        .prop("checked", this.checked);
});


$("#btnSaveSDGWeigh").click(function () {

    let tickets = [];

    $(".chkTicket:checked:not(:disabled)").each(function () {
        tickets.push($(this).val());
    });

    if (tickets.length == 0) {
        alert("Please select SDGWeigh data");
        return;
    }

    let payload = {
        memoNo: $("#MemoNo").val(),
        millCode: $("#MillCode").val(),
        tickets: tickets
    };

    console.log(payload);

    $.ajax({
        url: "/Memo/CheckSDGWeigh",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function () {
            alert("SDGWeigh data saved successfully");
            window.close();
        },
        error: function (xhr) {
            alert(xhr.responseText);
        }
    });
});


function formatDate(value) {

    if (!value)
        return "";

    let dt = new Date(value);

    const month = [
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    ];

    return ("0" + dt.getDate()).slice(-2)
        + "-"
        + month[dt.getMonth()]
        + "-"
        + dt.getFullYear();
}