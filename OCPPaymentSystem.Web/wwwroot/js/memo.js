$(document).ready(function () {
    loadCompany();
    loadSupplier();
    loadMemo();

    if (currentUser.approvalLevel == 0) {
        submissionMode();
    } else {
        approvalMode();
    }

    $("#Invoice").click(function () {

        downloadAttachment(
            "Invoice");

    });


    $("#BAP").click(function () {

        downloadAttachment(
            "BAP");

    });


    $("#FakturPajak").click(function () {

        downloadAttachment(
            "FakturPajak");

    });
});

$(document).on("click", "#tblMemo tr", function () {
        loadDetail(
            $(this).attr("data-id"));
    });

$(document).on("click","#btnSave", function () {
        saveMemo();
    });

$("#btnDelete").click(function () {
    DeleteMemo();
});

$("#btnSubmit").click(function () {
    SubmitMemo();
});

$("#btnApprove").click(function () {
    approveMemo();
});

$(document).on("click", "#btnNew", function () {
    clearForm();
});

$(document).on("click", "#btnInvoice", function () {
    if ($("#MemoNo").val() == "") {
        alert("Please save Memo first.");
        return;
    }
    $("#fileInvoice").click();
});

$(document).on("click", "#btnBAP", function () {
    if ($("#MemoNo").val() == "") {
        alert("Please save Memo first.");
        return;
    }
    $("#fileBAP").click();
});

$(document).on("click", "#btnFakturPajak", function () {
    if ($("#MemoNo").val() == "") {
        alert("Please save Memo first.");
        return;
    }
    $("#fileFakturPajak").click();
});

$("#fileInvoice").change(function () {

    uploadAttachment(
        this.files[0],
        "Invoice");

});

$("#fileBAP").change(function () {

    uploadAttachment(
        this.files[0],
        "BAP");

});

$("#fileFakturPajak").change(function () {

    uploadAttachment(
        this.files[0],
        "FakturPajak");

});

function loadCompany() {
    $.ajax({
        url: "/Memo/Company",
        type: "GET",
        success: function (data) {
            $("#Company").empty();
            $("#Company").append(
                "<option value=''>-- Select Company --</option>");

            $.each(data, function (i, item) {
                $("#Company").append(
                    "<option value='" +
                    item.code +
                    "'>" +
                    item.code +
                    " - " +
                    item.name +
                    "</option>");
            });
        }
    });
}

function loadSupplier() {
    $.get("/Memo/Supplier", function (data) {
        $("#Supplier").empty();
        $.each(data, function (i, x) {
            $("#Supplier").append(
                $("<option>")
                    .val(x.code)
                    .text(x.name)
            );
        });
    });
}

function DeleteMemo() {

    if ($("#MemoNo").val() == "") {
        alert("No Memo selected.");
        return;
    }

    if (!confirm("Delete this Memo ?"))
        return;

    $.ajax({
        url: "/Memo/Delete",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MemoNo: $("#MemoNo").val()
        }),

        success: function (result) {
            if (!result.success) {
                alert(result.message);
                return;
            }

            alert("Memo deleted successfully.");
            NewMemo();
            loadMemo();
            submissionMode();
        },

        error: function (xhr) {
            alert(xhr.responseText);
        }
    });

}

function NewMemo() {
    clearForm();
    $("#btnInvoice").hide();
    $("#btnBAP").hide();
    $("#btnFakturPajak").hide();
}

function loadMemo() {
    $.get("/Memo/MemoList", function (data) {
        $("#tblMemo").empty();
        $.each(data, function (i, x) {
            var tr = $("<tr>");

            tr.append("<td>" + (i + 1) + "</td>");
            tr.append("<td>" + x.companyCode + "</td>");
            tr.append("<td>" + x.supplierName + "</td>");

            tr.append("<td>" + formatDate(x.memoDate) + "</td>");
            tr.append("<td style='text-align:right'>" +
                Number(x.amount).toLocaleString() +
                "</td>");
            tr.append("<td>" + x.memoNo + "</td>");
            tr.append("<td>" + x.approvalStatus + "</td>");
            tr.attr("data-id", x.memoNo);

            $("#tblMemo").append(tr);
        });
    });
}

function loadDetail(memoNo) {
    $.get(
        "/Memo/Detail",
        {
            memoNo: memoNo
        },

        function (x) {
            $("#MemoNo").val(x.memoNo);
            $("#Company").val(x.companyCode);
            $("#Supplier").val(x.supplierCode);
            $("#MillCode").val(x.millCode);

            let dt = new Date(x.memoDate);
            let month = String(dt.getMonth() + 1).padStart(2, '0');
            let day = String(dt.getDate()).padStart(2, '0');
            let year = dt.getFullYear();

            $("#MemoDate").val(`${year}-${month}-${day}`);
            $("#Amount").val(x.amount);
            $("#Remarks").val(x.remarks);
            $("#Invoice").val(x.invoice);
            $("#BAP").val(x.bap);
            $("#FakturPajak").val(x.fakturPajak);
            $("#ApprovalLevel").val(x.ApprovalLevel);
            if (x.approvalLevel == 0) {
                submissionMode();
            } else {
                approvalMode();
            }

            $("#btnInvoice").show();
            $("#btnBAP").show();
            $("#btnFakturPajak").show();

            loadAttachment();
            loadSDGWeighDetail();
        });

}
function approveMemo() {
    if ($("#MemoNo").val() == "") {
        alert("No memo selected.");
        return;
    }

    if (!confirm("Approve this memo?"))
        return;

    $.ajax({
        url: "/Memo/Approve",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            memoNo: $("#MemoNo").val()
        }),

        success: function (result) {
            if (!result.success) {
                alert(result.message);
                return;
            }

            alert("Memo approved.");
            loadDetail($("#MemoNo").val());
            loadMemo();
        },

        error: function (xhr) {
            alert(xhr.responseText);
        }
    });
}

function approvalMode() {
    $("#Company").prop("disabled", true);
    $("#Supplier").prop("disabled", true);
    $("#MemoDate").prop("disabled", true);
    $("#Amount").prop("disabled", true);
    $("#Remarks").prop("disabled", true);

    /*
    $("#btnNew").prop("disabled", false);
    $("#btnSave").prop("disabled", false);
    $("#btnSubmit").prop("disabled", false);
    $("#btnDelete").prop("disabled", false);
    $("#btnApprove").prop("disabled", true);
    $("#btnReject").prop("disabled", true);
    */

    $("#btnNew").hide();
    $("#btnSave").hide();
    $("#btnDelete").hide();
    $("#btnSubmit").hide();

    $("#btnApprove").show();
    $("#btnReject").show();
}
function submissionMode() {
    $("#Company").prop("disabled", false);
    $("#Supplier").prop("disabled", false);
    $("#MemoDate").prop("disabled", false);
    $("#Amount").prop("disabled", false);
    $("#Remarks").prop("disabled", false);

    /*
    $("#btnNew").prop("disabled", true);
    $("#btnSave").prop("disabled", true);
    $("#btnSubmit").prop("disabled", true);
    $("#btnDelete").prop("disabled", true);
    $("#btnApprove").prop("disabled", false);
    $("#btnReject").prop("disabled", false);
    */

    $("#btnNew").show();
    $("#btnSave").show();
    $("#btnDelete").show();
    $("#btnSubmit").show();

    $("#btnApprove").hide();
    $("#btnReject").hide();
}

function rejectMemo() {
    if ($("#MemoNo").val() == "") {
        alert("No Memo selected.");
        return;
    }

    let remarks = prompt("Reject Remarks :");

    if (remarks == null)
        return;

    if (remarks.trim() == "") {
        alert("Reject remarks is required.");
        return;
    }

    $.ajax({
        url: "/Memo/Reject",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MemoNo: $("#MemoNo").val(),
            Remarks: remarks
        }),

        success: function (result) {
            if (!result.success) {
                alert(result.message);
                return;
            }

            alert("Memo rejected.");
            loadDetail($("#MemoNo").val());
            loadMemo();
        },

        error: function (xhr) {
            alert(xhr.responseText);
        }
    });
}
function clearForm() {
    $("#MemoNo").val("");
    $("#Company").prop("selectedIndex", 0);
    $("#Supplier").prop("selectedIndex", 0);
    $("#MemoDate").val("");
    $("#Amount").val("");
    $("#Remarks").val("");
    $("#Invoice").val("");
    $("#BAP").val("");
    $("#FakturPajak").val("");
}

$("#btnReject").click(function () {
    rejectMemo();
});

function saveMemo() {
    if ($("#Company").val() == "") {
        alert("Please select Company.");
        return;
    }

    if ($("#Supplier").val() == "") {
        alert("Please select Supplier.");
        return;
    }

    if ($("#MemoDate").val() == "") {
        alert("Please select Date.");
        return;
    }

    if ($("#Amount").val() == "") {
        alert("Amount cannot be empty.");
        return;
    }

    //save
    let formData =
        new FormData();

    formData.append(
        "MemoNo",
        $("#MemoNo").val());

    formData.append(
        "CompanyCode",
        $("#Company").val());

    formData.append(
        "SupplierCode",
        $("#Supplier").val());

    formData.append(
        "MemoDate",
        $("#MemoDate").val());

    formData.append(
        "Amount",
        $("#Amount").val());

    formData.append(
        "Remarks",
        $("#Remarks").val());

    if ($("#fileInvoice")[0].files.length > 0) {
        formData.append(
            "Invoice",
            $("#fileInvoice")[0].files[0]);
    }

    if ($("#fileBAP")[0].files.length > 0) {
        formData.append(
            "BAP",
            $("#fileBAP")[0].files[0]);
    }

    if ($("#fileFakturPajak")[0].files.length > 0) {
        formData.append(
            "FakturPajak",
            $("#fileFakturPajak")[0].files[0]);
    }

    var data =
    {
        companyCode: $("#Company").val(),
        supplierCode: $("#Supplier").val(),
        memoDate: $("#MemoDate").val(),
        amount: $("#Amount").val(),
        remarks: $("#Remarks").val(),
        memoNo: $("#MemoNo").val(),
        millCode: $("#MillCode").val()
    };

    let url =
        $("#MemoNo").val() == ""
            ? "/Memo/Save"
            : "/Memo/Update";

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(data),

        success: function (result) {
            if (!result.success) {
                alert(result.message);

                return;
            }

            alert("Data saved successfully.");

            loadMemo();
            loadDetail($("#MemoNo").val());
        },

        error: function (xhr) {
            console.log(xhr);

            alert(
                "Status : " + xhr.status +
                "\n\n" +
                xhr.responseText
            );
        }
    });

}

function loadAttachment() {

    let memoNo =
        $("#MemoNo").val();


    if (memoNo == "")
        return;


    $.get(
        "/Memo/AttachmentList",
        {
            memoNo: memoNo
        },

        function (data) {
            $("#Invoice").val("");
            $("#BAP").val("");
            $("#FakturPajak").val("");

            $.each(
                data,
                function (i, x) {
                    if (x.documentType == "Invoice")
                        $("#Invoice").val(x.fileName);

                    if (x.documentType == "BAP")
                        $("#BAP").val(x.fileName);

                    if (x.documentType == "FakturPajak")
                        $("#FakturPajak").val(x.fileName);
                });

        });

}
function formatDate(value) {
    if (!value)
        return "";

    let dt = new Date(value);

    const month =
        [
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        ];

    return ("0" + dt.getDate()).slice(-2)
        + "-"
        + month[dt.getMonth()]
        + "-"
        + dt.getFullYear();
}

function SubmitMemo() {
    if ($("#txtMemoNo").val() == "") {
        alert("Please save Memo first.");
        return;
    }

    if (!confirm("Submit this Memo for Approval?"))
        return;

    $.ajax({
        url: "/Memo/Submit",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MemoNo: $("#MemoNo").val()
        }),

        success: function (result) {
            if (result.success) {
                alert("Memo submitted successfully.");
                loadDetail($("#MemoNo").val());
                loadMemo();
            }
            else {
                alert(result.message);
            }
        },

        error: function (xhr) {
            alert(xhr.responseText);
        }
    });
}

$(document).ready(function () {


    if ($("#OpenMemoNo").val() != "") {


        loadDetail(
            $("#OpenMemoNo").val()
        );


    }


});

function uploadAttachment(file, type) {

    if (file == null) {
        alert("Please select file.");
        return;
    }

    let data =
        new FormData();

    data.append(
        "file",
        file,
        file.name);

    data.append(
        "memoNo",
        $("#MemoNo").val());

    data.append(
        "type",
        type);

    $.ajax({
        url:
            "/Memo/UploadAttachment",
        type:
            "POST",
        data:
            data,
        processData:
            false,
        contentType:
            false,

        success: function (result) {

            console.log(result);


            if (result.success) {

                alert(
                    "Attachment uploaded successfully");


                if (type == "Invoice") {
                    $("#Invoice").val(file.name);
                }


                if (type == "BAP") {
                    $("#BAP").val(file.name);
                }


                if (type == "FakturPajak") {
                    $("#FakturPajak").val(file.name);
                }

            }
            else {
                alert(result.message);
            }

        },

        error: function (xhr) {
            console.log(xhr);
            alert(xhr.responseText);
        }
    });
}

function downloadAttachment(type) {

    if ($("#MemoNo").val() == "") {
        return;
    }


    window.open(
        "/Memo/DownloadAttachment?memoNo="
        + $("#MemoNo").val()
        + "&type="
        + type,
        "_blank"
    );

}

$("#btnSDGWeigh").click(function () {

    let memoNo =
        $("#MemoNo").val();


    if (memoNo == "") {
        alert(
            "Save Memo first");
        return;
    }


    window.open(
        "/Memo/SDGWeighChecking"
        + "?memoNo="
        + memoNo

        + "&supplierCode="
        + $("#Supplier").val()

        + "&millCode="
        + $("#MillCode").val(),

        "_blank"
    );

});

$("#btnSDGWeigh").click(function () {


    if ($("#MemoNo").val() == "") {

        alert(
            "Please save Memo first.");

        return;
    }


    window.open(

        "/Memo/SDGWeighChecking"
        + "?memoNo="
        + $("#MemoNo").val()

        + "&supplierCode="
        + $("#Supplier").val()

        + "&millCode="
        + $("#MillCode").val(),

        "_blank"

    );


});

function loadSDGWeighDetail() {
    let memoNo =
        $("#MemoNo").val();
    if (memoNo == "")
        return;

    $.ajax({
        url:
            "/Memo/SDGWeighDetail",
        type:
            "GET",
        data:
        {
            memoNo: memoNo
        },

        success: function (data) {
            $("#sdgDetailList").empty();

            let totalNett = 0;
            let totalDeduction = 0;
            //console.table(data);

            $.each(data, function (i, x) {
                totalNett +=
                    x.nettWeight;
                totalDeduction +=
                    x.deD_WT;
                $("#sdgDetailList")
                    .append(`
<tr>
<td>
${formatDate(x.postDate)}
</td>
<td>
${x.serialNo}
</td>
<td>
${x.lorryNo}
</td>
<td>
${x.driverCode}
</td>
<td class="text-end">
${x.bunchWeight}
</td>

<td class="text-end">
${formatNumber(x.weightIn)}
</td>

<td class="text-end">
${formatNumber(x.weightOut)}
</td>

<td class="text-end">
${formatNumber(x.nettWeight)}
</td>

<td class="text-end">
${formatNumber(x.deD_WT)}
</td>

</tr>
`);
            });

            $("#sdgSummary")
                .html(
                   `
Ticket : ${data.length}
&nbsp;&nbsp;

Nett :
${formatNumber(totalNett)}
KG

&nbsp;&nbsp;

Deduction :
${formatNumber(totalDeduction)}
KG
`
                );
        }
    });
}

function formatNumber(x) {

    if (x == null)
        return "0";


    return x.toLocaleString();

}