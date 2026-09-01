$(document).ready(function () {
    loadCompany();
    $("#btnSearch").click(function () {
        searchMemo();
    });
});

function loadCompany() {
    $.get("/MemoSearch/Company",
        function (data) {
            $("#Company").empty();
            $("#Company").append(
                "<option value=''>-</option>"
            );
            $.each(data, function (i, x) {
                $("#Company").append(
                    `<option value='${x.code}'>
                        ${x.code} - ${x.name}
                    </option>`
                );
            });
        });
}

function searchMemo() {
    if ($("#Company").val() == "")
        return;

    $.ajax({
        url: "/MemoSearch/Search",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({

            companyCode:
                $("#Company").val(),

            supplierCode:
                $("#Supplier").val(),


            dateFrom:
                $("#DateFrom").val() == ""
                    ? null
                    : $("#DateFrom").val(),


            dateTo:
                $("#DateTo").val() == ""
                    ? null
                    : $("#DateTo").val(),


            amountFrom:
                $("#AmountFrom").val() == ""
                    ? null
                    : parseFloat($("#AmountFrom").val()),


            amountTo:
                $("#AmountTo").val() == ""
                    ? null
                    : parseFloat($("#AmountTo").val()),


            remarks:
                $("#Remarks").val()

        }),
        success: function (data) {
            console.table(data);
            $("#memoResult").empty();
            $.each(data, function (i, x) {
                $("#memoResult").append(`
        <tr ondblclick="openMemo('${x.memoNo}')"style="cursor:pointer">
            <td>${x.memoNo}</td>
            <td>${formatDateDDMMMYYYY(x.memoDate)}</td>
            <td>${x.companyCode}</td>
            <td>${x.supplierName}</td>
            <td class="text-end">
                ${formatAmount(x.amount)}
            </td>
            <td>${x.approvalStatus}</td>
            <td>${x.remarks}</td>
            <td><a href="#" onclick="openAttachment('${x.memoNo}','Memo')">${x.memo ?? ''}</a></td>
            <td><a href="#" onclick="openAttachment('${x.memoNo}','Invoice')">${x.invoice ?? ''}</a></td>
            <td><a href="#" onclick="openAttachment('${x.memoNo}','BAP')">${x.bap ?? ''}</a></td>
            <td><a href="#" onclick="openAttachment('${x.memoNo}','FakturPajak')">${x.fakturPajak ?? ''}</a></td>
            </tr>
        `);
        });
        }
    });
}

$(document).ready(function () {
    loadCompany();
    $("#Company").change(function () {
        loadSupplier();
    });

    $("#btnSearch").click(function () {
        searchMemo();
    });
});

function loadSupplier() {
    $("#Supplier").empty();
    $("#Supplier").append(
        "<option value=''>-</option>"
    );

    if ($("#Company").val() == "")
        return;

    $.ajax({
        url: "/Memo/Supplier",
        type: "POST",
        contentType: "application/json",

        data: JSON.stringify({
            companyCode:
                $("#Company").val()

        }),

        success: function (data) {
            $.each(data, function (i, x) {
                $("#Supplier").append(
                    `<option value='${x.code}'>${x.code} - ${x.name}</option>`
                );
            });
        }
    });
}

function openMemo(memoNo) {


    window.location.href =

        "/Memo/Index?memoNo=" + Uri.EscapeDataString(memoNo);


}

function formatDateMMDDYYYY(value) {

    if (!value)
        return "";


    let date = new Date(value);


    let month =
        String(date.getMonth() + 1)
            .padStart(2, "0");


    let day =
        String(date.getDate())
            .padStart(2, "0");


    let year =
        date.getFullYear();


    return month + "/" + day + "/" + year;

}

function formatAmount(value) {

    if (value == null || value == "")
        return "0.00";


    return Number(value)
        .toLocaleString(
            "en-US",
            {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }
        );

}

function formatDateDDMMMYYYY(value) {

    if (!value)
        return "";


    let date = new Date(value);


    let day =
        String(date.getDate())
            .padStart(2, "0");


    let monthNames =
        [
            "Jan",
            "Feb",
            "Mar",
            "Apr",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sep",
            "Oct",
            "Nov",
            "Dec"
        ];


    let month =
        monthNames[date.getMonth()];


    let year =
        date.getFullYear();


    return day + "-" + month + "-" + year;

}

function openAttachment(
    memoNo,
    type) {

    window.open(
        "/Memo/DownloadAttachment?memoNo="
        + memoNo
        + "&type="
        + type,
        "_blank"
    );

}