const FORM_MODE = {
    NEW: 1,
    HEADER: 2,
    DETAIL: 3,
    APPROVAL: 4
};

let currentMode = FORM_MODE.NEW;

function setMode(mode) {

    currentMode = mode;

    //-------------------------
    // HEADER
    //-------------------------

    $("#Company").prop("disabled", mode != FORM_MODE.NEW);
    $("#Supplier").prop("disabled", mode != FORM_MODE.NEW);
    $("#MemoDate").prop("disabled", mode != FORM_MODE.NEW);

    //-------------------------
    // DETAIL
    //-------------------------

    $("#btnSDGWeigh").prop("disabled", mode == FORM_MODE.NEW);

    $("#NettWeight").prop("disabled", true);
    $("#Deduction").prop("disabled", true);

    $("#PricePerKg").prop("disabled", mode != FORM_MODE.DETAIL);

    $("#SubTotal").prop("disabled", true);
    $("#Amount").prop("disabled", true);
    $("#PPN").prop("disabled", true);
    $("#PPH").prop("disabled", true);

    //$("#cbPPN").prop("disabled", true);
    $("#cbPPN").prop("disabled", mode != FORM_MODE.NEW);

    //$("#Remarks").prop("disabled", mode != FORM_MODE.NEW);
    $("#Remarks").prop("disabled", mode != FORM_MODE.DETAIL);

    //-------------------------
    // ATTACHMENT
    //-------------------------

    $("#btnInvoice").prop("disabled", mode != FORM_MODE.DETAIL);
    $("#btnBAP").prop("disabled", mode != FORM_MODE.DETAIL);
    $("#btnFakturPajak").prop("disabled", mode != FORM_MODE.DETAIL);

    //-------------------------
    // BUTTON
    //-------------------------
    $("#btnDelete").prop("disabled", mode == FORM_MODE.NEW);
    $("#btnSubmit").prop("disabled", mode != FORM_MODE.DETAIL);

    if (currentUser.approvalLevel == 0) {
        submissionMode();
    } else {
        approvalMode();
    }
}

$(document).ready(function () {
    loadCompany();
    loadSupplier();
    loadMemo();

    clearForm();

    setMode(FORM_MODE.NEW);

    if (currentUser.approvalLevel == 0) {
        submissionMode();
    } else {
        approvalMode();
    }

    $("#Invoice").click(function () {downloadAttachment("Invoice");});
    $("#BAP").click(function () {downloadAttachment("BAP");});
    $("#FakturPajak").click(function () {downloadAttachment("FakturPajak");});
});

$(document).on("click", "#tblMemo tr", function () {loadDetail($(this).attr("data-id"));});
$(document).on("click","#btnSave", function () {saveMemo();});
$("#btnDelete").click(function () {DeleteMemo();});
$("#btnSubmit").click(function () {SubmitMemo();});
$("#btnApprove").click(function () {approveMemo();});

$(document).on("click", "#btnNew", function () {
    clearForm();
    setMode(FORM_MODE.NEW);
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
        console.table(data);
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
            loadMemo();
            clearForm();
            setMode(FORM_MODE.NEW);
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
    setMode(FORM_MODE.NEW);
}

function loadMemo() {
    $.get("/Memo/MemoList", function (data) {
        console.table(data);
        $("#tblMemo").empty();
        $.each(data, function (i, x) {
            var tr = $("<tr>");

            tr.append("<td>" + (i + 1) + "</td>");
            tr.append("<td>" + x.companyCode + "</td>");
            tr.append("<td>" + x.millAbbv + "</td>");
            tr.append("<td>" + x.supplierName + "</td>");

            tr.append("<td>" + formatDate(x.memoDate) + "</td>");
            tr.append("<td style='text-align:right'>" + Number(x.amount).toLocaleString('en-US', { maximumFractionDigits: 0 }) + "</td>");
            if (x.memo && x.memo.trim() !== "") {
                tr.append("<td><a href=\"#\" onclick=\"openAttachment('" + x.memoNo + "', 'Memo')\">" + x.memoNo + "</a></td>");
            } else {
                tr.append("<td>" + x.memoNo + "</td>");
            }
            tr.append("<td>" + x.approvalStatus + " - " + x.approvalRemarks + "</td>");
            tr.attr("data-id", x.memoNo);

            $("#tblMemo").append(tr);
        });
    });
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
function loadDetail(memoNo) {
    $.get(
        "/Memo/Detail",
        {
            memoNo: memoNo
        },

        function (x) {
            console.table(x);

            $("#MemoNo").val(x.memoNo);
            $("#Company").val(x.companyCode);
            $("#Supplier").val(x.supplierCode);
            $("#MillCode").val(x.millCode);

            loadBank(x.bankCode);

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

            $("#Perihal").val(x.perihal);
            $("#InvoiceNo").val(x.invoiceNo);
            $("#PricePerKg").val(x.pricePerKg);
            $("#PPN").val(x.ppn);

            let weight = (parseFloat(x.nettWeight) || 0) - (parseFloat(x.deduction) || 0);
            let price = parseFloat(x.pricePerKg) || 0;
            let ppn = parseFloat(x.ppn) || 0;

            let subTotal = weight * price;

            if (subTotal > 0 && ppn > 0) {
                let ppnRate = (ppn / subTotal) * 100;
                setClosestPPNRate(ppnRate);
            }

            $("#PPH").val(x.pph);

            $("#btnInvoice").show();
            $("#btnBAP").show();
            $("#btnFakturPajak").show();

            loadAttachment();
            loadSDGWeighDetail();

            $("#isDeduction").prop("checked", (x.deduction ?? 0) > 0);
            $("#isPPN").prop("checked", (x.ppn ?? 0) > 0);
            $("#isPPH").prop("checked", (x.pph ?? 0) > 0);

            calculateAmount();  

            if (x.approvalStatus == 'Rejected') {
                $("#btnApprove").prop("disabled", true);
                $("#btnReject").prop("disabled", true);
            } else {
                $("#btnApprove").prop("disabled", false);
                $("#btnReject").prop("disabled", false);
                setMode(FORM_MODE.HEADER);
            }
    });
}
function setClosestPPNRate(rate) {
    let closest = null;
    let minDiff = Infinity;

    $("#cbPPN option").each(function () {
        let value = parseFloat($(this).val());
        let diff = Math.abs(value - rate);

        if (diff < minDiff) {
            minDiff = diff;
            closest = value;
        }
    });

    if (closest !== null) {
        $("#cbPPN").val(closest);
    }
}
function approveMemo() {
    if ($("#MemoNo").val() == "") {
        alert("No memo selected.");
        return;
    }

    //if (!confirm("Approve this memo?"))
    //    return;

    let remarks = prompt("Approve this memo :");

    if (remarks == null)
        return;

    if (remarks.trim() == "") {
        alert("Approve remarks is required.");
        return;
    }

    $.ajax({
        url: "/Memo/Approve",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            memoNo: $("#MemoNo").val(),
            Remarks: remarks
        }),

        success: function (result) {
            if (!result.success) {
                alert(result.message);
                return;
            }

            alert("Memo approved.");
            loadMemo();
            clearForm();
            setMode(FORM_MODE.NEW);
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
    $("#PricePerKg").prop("disabled", true);
    $("#Remarks").prop("disabled", true);

    $("#Perihal").prop("disabled", true);
    $("#InvoiceNo").prop("disabled", true);
    $("#cbBank").prop("disabled", true);

    $("#isDeduction").prop("disabled", true);
    $("#isPPN").prop("disabled", true);
    $("#isPPH").prop("disabled", true);
    $("#btnSDGWeigh").prop("disabled", true);

    $("#cbPPN").prop("disabled", true);

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
    //$("#Amount").prop("disabled", false);
    $("#Remarks").prop("disabled", false);
    //$("#PricePerKg").prop("disabled", false);

    $("#Perihal").prop("disabled", false);
    $("#InvoiceNo").prop("disabled", false);
    $("#cbBank").prop("disabled", false);

    $("#isDeduction").prop("disabled", false);
    $("#isPPN").prop("disabled", false);
    $("#isPPH").prop("disabled", false);
    $("#btnSDGWeigh").prop("disabled", false);

    $("#cbPPN").prop("disabled", false);

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
            loadMemo();
            clearForm();
            setMode(FORM_MODE.NEW);
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

    $("#NettWeight").val("");
    $("#Deduction").val("");
    $("#PricePerKg").val("");
    $("#SubTotal").val("");
    $("#PPN").val("");
    $("#PPH").val("");

    $("#isDeduction").prop("checked", false);
    $("#isPPN").prop("checked", false);
    $("#isPPH").prop("checked", false);

    $("#Perihal").val("Pembelian TBS");
    $("#InvoiceNo").val("");

    $("#cbBank").empty();
    $("#cbBank").append(
        $("<option>")
            .val("")
            .text("-- Select Bank Account --")
    );
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
        $("#Amount").val(0);
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

    formData.append(
        "NettWeight",
        $("#NettWeight").val());

    formData.append(
        "Deduction",
        $("#Deduction").val());

    formData.append(
        "PricePerKg",
        $("#PricePerKg").val());

    formData.append(
        "PPN",
        $("#PPN").val());

    formData.append(
        "PPH",
        $("#PPH").val());

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

    let deduction = $("#isDeduction").is(":checked")
        ? (parseFloat($("#Deduction").val()) || 0)
        : 0;

    let pph = $("#isPPH").is(":checked")
        ? (parseFloat($("#PPH").val()) || 0)
        : 0;

    let ppn = $("#isPPN").is(":checked")
        ? (parseFloat($("#PPN").val()) || 0)
        : 0;

    var data =
    {
        companyCode: $("#Company").val(),
        supplierCode: $("#Supplier").val(),
        memoDate: $("#MemoDate").val(),

        perihal: $("#Perihal").val(),
        invoiceNo: $("#InvoiceNo").val(),
        bankCode: $("#cbBank").val(),

        amount: parseFloat($("#Amount").val()) || 0,
        remarks: $("#Remarks").val(),
        memoNo: $("#MemoNo").val(),
        millCode: $("#MillCode").val(),

        nettWeight: parseFloat($("#NettWeight").val()) || 0,
        deduction: deduction,
        pricePerKg: parseFloat($("#PricePerKg").val()) || 0,
        pph: pph,
        ppn: ppn
    };

    console.table(data);
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
            clearForm();
            setMode(FORM_MODE.NEW);
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
    let memoNo = $("#MemoNo").val();
    if (memoNo == "") return;

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
                //loadDetail($("#MemoNo").val());
                //loadMemo();
                loadMemo();
                clearForm();
                setMode(FORM_MODE.NEW);
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

    let data = new FormData();

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
    let memoNo = $("#MemoNo").val();

    if (memoNo == "") {
        alert("Save Memo first");
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

    //harus 2x langkah
    //loadSDGWeighDetail();
    //calculateAmount();
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
                totalNett += x.nettWeight;
                totalDeduction += x.deD_WT;
                $("#sdgDetailList")
                    .append(`
                        <tr>
                            <td>${formatDate(x.postDate)}</td>
                            <td>${x.serialNo}</td>
                            <td>${x.lorryNo}</td>
                            <td>${x.driverCode}</td>
                            <td class="text-end">${x.bunchWeight}</td>
                            <td class="text-end">${formatNumber(x.weightIn)}</td>
                            <td class="text-end">${formatNumber(x.weightOut)}</td>
                            <td class="text-end">${formatNumber(x.nettWeight)}</td>
                            <td class="text-end">${formatNumber(x.deD_WT)}</td>
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
            $("#NettWeight").val(totalNett);
            $("#Deduction").val(totalDeduction);

            calculateAmount(); 

            setMode(FORM_MODE.DETAIL);
        }
    });
}

$("#PricePerKg").on("keyup change", calculateAmount);
$("#isPPN").change(calculateAmount);
$("#isPPH").change(calculateAmount);
$("#isDeduction").change(calculateAmount);
$("#cbPPN").on("change", calculateAmount);

$("#Supplier").on("change", function () { loadBank(); });
$("#Company").on("change", function () { loadBank(); });

function loadBank(selectedBankCode = "") {
    let supplierCode = $("#Supplier").val();
    $("#cbBank").empty();
    $("#cbBank").append(
        $("<option>")
            .val("")
            .text("-- Select Bank Account --")
    );

    if (!supplierCode) {
        return;
    }

    $.get(
        "/Memo/Bank",
        {
            supplierCode: supplierCode,
            millCode: $("#MillCode").val()
        },
        function (data) {
            console.table(data);
            $.each(data, function (i, x) {
                $("#cbBank").append(
                    $("<option>")
                        .val(x.code)
                        .text(
                            x.bankName +
                            " - " +
                            x.bankAccountNo
                        )
                );
            });

            if (selectedBankCode) {
                $("#cbBank").val(selectedBankCode);
            }
        }
    );
}
function calculateAmount() {
    let nett = parseFloat($("#NettWeight").val()) || 0;
    let deduction = parseFloat($("#Deduction").val()) || 0;
    let price = parseFloat($("#PricePerKg").val()) || 0;
    let weight = nett;

    if ($("#isDeduction").is(":checked")) weight -= deduction;

    let subTotal = weight * price;
    let ppn = 0;
    let pph = 0;

    if ($("#isPPN").is(":checked")) {
        let ppnRate = parseFloat($("#cbPPN").val()) || 0;
        ppn = subTotal * ppnRate / 100;
    }

    if ($("#isPPH").is(":checked")) pph = subTotal * 0.0025;

    $("#SubTotal").val(subTotal.toFixed(2));
    $("#PPN").val(ppn.toFixed(2));
    $("#PPH").val(pph.toFixed(2));
    $("#Amount").val((subTotal + ppn - pph).toFixed(2));
}

function formatNumber(x) {
    if (x == null) return "0";

    return x.toLocaleString();
}