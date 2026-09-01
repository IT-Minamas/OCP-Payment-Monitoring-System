USE OCPPaymentSystem

DROP TABLE tbdPriceRange
CREATE TABLE tbdPriceRange
(
    fldID INT IDENTITY(1,1) PRIMARY KEY,

    fldMillCode VARCHAR(20) NOT NULL,

    fldDateFrom DATE NOT NULL,
    fldDateTo DATE NOT NULL,

    fldPriceFrom DECIMAL(18,2) NOT NULL,
    fldPriceTo DECIMAL(18,2) NOT NULL,

    fldAttachmentFileName NVARCHAR(255) NULL,
    fldAttachmentContentType NVARCHAR(100) NULL,
    fldAttachmentFileSize BIGINT NULL,
    fldAttachment VARBINARY(MAX) NULL,

    fldCreatedBy NVARCHAR(100) NOT NULL,
    fldCreatedOn DATETIME NOT NULL DEFAULT(GETDATE()),
    fldCreatedIP NVARCHAR(50) NOT NULL,

    fldUpdatedBy NVARCHAR(100) NULL,
    fldUpdatedOn DATETIME NULL,
    fldUpdatedIP NVARCHAR(50) NULL
);

USE OCPPaymentSystem
SELECT * FROM tbdPriceRange
DELETE FROM tbdPriceRange

SELECT * FROM vw_Company
USE OCPPaymentSystem
SELECT * FROM vw_SearchMemo
SELECT * FROM tbdMemo
SELECT * FROM tbdApproval
DELETE FROM tbdMemo
SELECT * FROM tbdMemoDetail
SELECT * FROM vw_SearchMemo AS a LEFT JOIN tbdMemoAttachment AS b ON a.fldNo=b.fldNo
SELECT * FROM tbdMemoAttachment
DELETE FROM tbdMemoAttachment
SELECT * FROM tbdApproval
SELECT * FROM tbdApprovalLevel
SELECT * FROM tbdApprovalStatus
SELECT * FROM tbdEmailRecipient
SELECT * FROM tbdEmailTemplate
SELECT * FROM tbdLog
SELECT * FROM tbdMemoLog
SELECT * FROM tbdRunningNumber
--current approval nya salah
DELETE FROM tbdApproval WHERE fldNo='MEM20260700001'

SELECT * FROM SAP_LFA1


SELECT a.LIFNR AS fldCode,a.NAME1 AS fldName,a.ORT01 AS fldAddress1,a.ORT02 AS fldAddress2,a.TELF1 AS fldPhone1,a.TELFX AS fldPhone2,a.STCEG AS fldNPWP,a.STCD3 AS fldSKN,
       b.BANKN AS fldBankAccountNo,b.KOINH AS fldNameOnBankAccount,c.BANKA AS fldBankName,c.STRAS AS fldBankAddress1,c.ORT01 AS fldBankAddress1
FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SAP_LFA1] AS a
JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[SAP_LFBK] AS b ON a.LIFNR=b.LIFNR AND b.SOURCESAP='S4'
JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[SAP_BNKA] AS c ON b.BANKS=c.BANKS AND b.BANKL=c.BANKL AND b.SOURCESAP='S4'
WHERE a.SOURCESAP='S4' AND a.LAND1='ID'

SELECT TOP 10 * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SAP_LFBK]
SELECT TOP 10 * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SAP_BNKA]

SELECT DISTINCT Client_ID,SUPPLIER_CODE FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER]

SELECT * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER]
SELECT fldSAPVirtualCode AS fldCode,fldName,* FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[vw_UnitSetup2] WHERE fldIsActive=1 AND fldType='M'

SELECT TOP 10 * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_PURCHASE]

SELECT
	M.fldNo,
	M.fldCompanyCode,
	M.fldSupplierCode,
	S.SUPPLIER_NAME AS fldSupplierName,
	M.fldDate,
	M.fldAmount,
	M.fldRemarks,
	M.fldCreatedBy,
	M.fldCreatedOn,
	ISNULL(A.fldApprovalLevel,0) AS ApprovalLevel,
	CASE
		WHEN A.fldNo IS NULL THEN 'Draft'
		WHEN A.fldCurrentApproval = 1 THEN
			'Waiting Level ' + CAST(A.fldApprovalLevel AS VARCHAR)
		ELSE
			A.fldStatus
	END AS ApprovalStatus,
	A.fldApprovedBy,
	A.fldApprovedOn,
	A.fldCreatedOn AS ApprovalCreatedOn
FROM tbdMemo AS M
JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER] AS S ON M.fldSupplierCode=S.SUPPLIER_CODE COLLATE SQL_Latin1_General_CP1_CI_AS AND M.fldMillCode=S.Client_ID COLLATE SQL_Latin1_General_CP1_CI_AS
OUTER APPLY
(
    SELECT TOP 1 *
    FROM tbdApproval
    WHERE fldNo = M.fldNo
    ORDER BY
        fldCurrentApproval DESC,
        fldApprovalLevel DESC
) A
WHERE 1=1

USE OCPPaymentSystem
SELECT * FROM vw_SearchMemo
SELECT * FROM tbdMemo
SELECT * FROM tbdApproval
DELETE FROM tbdApproval WHERE fldNo='MEM20260600015'

SELECT * FROM tbdApprovalLevel

SELECT *
FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER]
WHERE SUPPLIER_CODE='E428'

--User ID
EXECUTE [CentralAuthentication].[dbo].[sp_GetEntityByUserAccess] 
   '00043628'
  ,68
GO

USE OCPPaymentSystem
DECLARE @MaxPeriod DATE;
SELECT @MaxPeriod = MAX(Period)
FROM [CentralAuthentication].dbo.tblManPower;
SELECT a.*,b.fldUnitCode,c.fldRegionCode,d.fldAreaCode
FROM [CentralAuthentication].dbo.tblManPower AS a
LEFT JOIN [CentralAuthentication].dbo.tblRoleApplication AS aa ON aa.fldUserId=a.Employee_ID AND aa.fldIdApp = '68'
LEFT JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_NADILocationMapping] AS b ON a.Location_Index=b.fldNADILocation
LEFT JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_NADIRegionMapping] AS c ON a.Region=c.fldNADIRegion
LEFT JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_NADIAreaMapping] AS d ON a.Area=d.fldNADIArea
WHERE a.Period=@MaxPeriod AND fldUnitCode='M393'

SELECT fldName
FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_Area]
WHERE fldName NOT IN
(
	SELECT a.Area
	FROM [CentralAuthentication].dbo.tblManPower AS a
	LEFT JOIN [CentralAuthentication].dbo.tblRoleApplication AS aa ON aa.fldUserId=a.Employee_ID AND aa.fldIdApp = '68'
	LEFT JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_NADILocationMapping] AS b ON a.Location_Index=b.fldNADILocation
	LEFT JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_NADIRegionMapping] AS c ON a.Region=c.fldNADIRegion
	LEFT JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_NADIAreaMapping] AS d ON a.Area=d.fldNADIArea
	WHERE a.Period='2026-06-01 00:00:00' AND a.Business_Title LIKE '%Area Controller%'
)

--00077780 Pradana Nayang - Treasury
--00043484 Iswanto - Accounting
--00043228 Yustinus Lambang - CFO

--M393 Sekunyir Factory
--00043445 Ondra Utama - RCEO
--00043628 Pandjaitan Novery Erpan - Area Controller
--00043652 Sutrisno - Manager
--00043901 Sapon Priyanto - Kepala Tata Usaha
--00303100 Isnanda Utama Harahap - Senior Assistant
SELECT * FROM vw_SearchMemo

--M438 Ungkaya Factory - tidak mempunyai AC
--00072231 - Yuliono - Cov Manager, Ungkaya Factory - POM
--00075224 - Amrin Naing - Kasie UKF
--00094663 - Sigit Winarto - Covering Senior Assistant UKF

--Role

--Company Code
--Approval Level
EXECUTE [CentralAuthentication].[dbo].[sp_GetDetailByUserAccess] 
   '00043445'
  ,68
GO
--ambil Business_Title-nya, lalu mapping-kan ke nomor approval di tblApprovalLevel
SELECT DISTINCT Business_Title
FROM [CentralAuthentication].dbo.tblManPower
WHERE Employee_Name LIKE '%pradana%'

SELECT *
FROM [CentralAuthentication].dbo.tblManPower
WHERE Employee_ID='00033035'

EXECUTE dbo.sp_GetCompanyAccess 10, 'TSA', 'Kalteng Barat', 'Kalteng Kalbar'

USE OCPPaymentSystem
SELECT * FROM tbdMemo
DELETE FROM tbdMemo
DELETE FROM tbdApproval
SELECT * FROM tbdApproval
SELECT * FROM vw_SearchMemo

--tombol submit harus disable dulu (done)
--list harus refresh setelah tombol submit ditekan (done)
--ketika AC mau approve/reject, kenapa suppliernya kosong di memo detail?

SELECT * FROM vw_SearchMemo
WHERE fldNo='MEM20260700002'
SELECT * FROM tbdApproval
WHERE fldNo='MEM20260700002'
DELETE FROM tbdApproval WHERE fldNo='MEM20260700002'

SELECT * FROM tbdApprovalLevel
SELECT * FROM tbdApprovalStatus
SELECT * FROM tbdEmailRecipient
SELECT * FROM tbdEmailTemplate
SELECT * FROM tbdLog
SELECT * FROM tbdMemoLog

SELECT * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_Region]
SELECT * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_Company]
SELECT * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_Area]
SELECT * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_NADIAreaMapping]
SELECT * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_NADIRegionMapping]

SELECT * FROM tbdApprovalLevel
Business_Title LIKE '%Chief Financial Officer%' --40
Business_Title LIKE '%Regional CEO%' --30
Business_Title LIKE '%Area Controller%' --20
Business_Title LIKE '%Manager%' --10
Business_Title LIKE '%Kasi%Adm%' --0


DECLARE @MaxPeriod DATE;
SELECT @MaxPeriod = MAX(Period)
FROM [CentralAuthentication].dbo.tblManPower;
SELECT b.fldUnitCode,a.*
FROM [CentralAuthentication].[dbo].tblManPower AS a
JOIN [CentralAuthentication].[dbo].tblRoleApplication AS aa ON aa.fldUserId=a.Employee_ID AND aa.fldIdApp = '68'
LEFT JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_NADILocationMapping] AS b ON a.Location_Index=b.fldNADILocation
WHERE a.Period=@MaxPeriod AND a.Employee_ID='00072231'

SELECT fldSAPVirtualCode FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[vw_UnitSetup2] WHERE fldCompanyCode='ITH' AND fldIsActive=1 AND fldType='M'
SELECT * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER]

SELECT * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[vw_UnitSetup2]

SELECT
a.fldID,
a.fldMillCode,
a.fldDateFrom,
a.fldDateTo,
a.fldPriceFrom,
a.fldPriceTo,
a.fldAttachmentFileName,
a.fldAttachmentContentType,
a.fldAttachmentFileSize,
a.fldCreatedBy,
a.fldCreatedOn,
a.fldCreatedIP,
a.fldUpdatedBy,
a.fldUpdatedOn,
a.fldUpdatedIP,
b.fldCompanyCode
FROM tbdPriceRange AS a
JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[vw_UnitSetup2] AS b ON a.fldMillCode COLLATE SQL_Latin1_General_CP1_CI_AS = b.fldSAPVirtualCode COLLATE SQL_Latin1_General_CP1_CI_AS
WHERE 1=1 

USE OCPPaymentSystem
SELECT
    m.fldNo MemoNo,
    m.fldDate MemoDate,
    c.fldName CompanyName,
    m.fldSupplierCode SupplierCode,
    s.fldName SupplierName,
    m.fldInvoice InvoiceNo,
    m.fldAmount Amount,
    m.fldRemarks Remarks,
    s.fldBankName,
    s.fldBankAccountNo,
    s.fldNameOnBankAccount,
    m.fldCreatedBy,
    m.fldCreatedOn,

	d.*,
	s.*
FROM tbdMemo m
INNER JOIN vw_Company c
    ON m.fldCompanyCode COLLATE Latin1_General_CI_AI=c.fldCode COLLATE Latin1_General_CI_AI
INNER JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER] AS d 
	ON m.fldMillCode COLLATE Latin1_General_CI_AI=d.Client_ID AND m.fldSupplierCode=d.SUPPLIER_CODE COLLATE Latin1_General_CI_AI
INNER JOIN vw_Supplier s
    ON d.Sap_Code COLLATE Latin1_General_CI_AI=s.fldCode COLLATE Latin1_General_CI_AI
WHERE m.fldNo='M.7/OP-TREA/BSC-TBS/VIII/2026'

SELECT * FROM vw_Supplier
WHERE fldCode='1001011078'

SELECT * FROM tbdMemo
SELECT * FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER]

SELECT a.Client_ID AS fldMillCode,a.SUPPLIER_CODE AS fldSupplierCode,a.SUPPLIER_NAME AS fldSupplierSDGWeighName,a.SAP_Code AS fldSAPCode
FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER] AS a
WHERE a.SAP_Code IS NOT NULL AND a.SAP_Code<>''

SELECT
    ApprovalLevel,
    REPLACE(
        ApprovalStatus,
        'Waiting for ',
        ''
    ) AS ApprovalName,
    COUNT(*) Total
FROM vw_SearchMemo
WHERE ApprovalLevel>0
GROUP BY
    ApprovalLevel,
    ApprovalStatus
ORDER BY
    ApprovalLevel

SELECT
    a.fldApprovalLevel,
    a.fldCreatedBy,
    d.Employee_Name AS fldCreatedName,
    a.fldCreatedOn,
    a.fldApprovedBy,
    c.Employee_Name AS fldApprovedName,
    a.fldApprovedOn,
    b.fldDescription AS ApprovalLevelDescription
FROM tbdApproval a
JOIN tbdApprovalLevel b
    ON a.fldApprovalLevel = b.fldApprovalLevel
OUTER APPLY
(
    SELECT TOP 1
        mp.Employee_Name
    FROM [CentralAuthentication].dbo.tblManPower mp
    WHERE a.fldApprovedBy COLLATE Latin1_General_CI_AI =
          mp.Employee_ID COLLATE Latin1_General_CI_AI
    ORDER BY mp.Period DESC
) c
OUTER APPLY
(
    SELECT TOP 1
        mp.Employee_Name
    FROM [CentralAuthentication].dbo.tblManPower mp
    WHERE a.fldCreatedBy COLLATE Latin1_General_CI_AI =
          mp.Employee_ID COLLATE Latin1_General_CI_AI
    ORDER BY mp.Period DESC
) d
WHERE a.fldNo = 'M.1/OP-TREA/GPI-TBS/VIII/2026'
ORDER BY a.fldApprovalLevel;

SELECT
    m.fldNo MemoNo,
    m.fldDate MemoDate,
    c.fldName CompanyName,
    m.fldSupplierCode SupplierCode,
    s.fldName SupplierName,
    m.fldInvoice InvoiceNo,
    m.fldAmount Amount,
    m.fldRemarks Remarks,
    m.fldPerihal Perihal,
    s.fldBankName,
    s.fldBankAccountNo,
    s.fldNameOnBankAccount,
    m.fldCreatedBy,
    m.fldCreatedOn,
	d.Sap_Code
FROM tbdMemo m
INNER JOIN vw_Company c ON m.fldCompanyCode COLLATE Latin1_General_CI_AI = c.fldCode COLLATE Latin1_General_CI_AI
INNER JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER] d ON d.Client_ID COLLATE Latin1_General_CI_AI = m.fldMillCode COLLATE Latin1_General_CI_AI AND m.fldSupplierCode COLLATE Latin1_General_CI_AI = d.SUPPLIER_CODE COLLATE Latin1_General_CI_AI
LEFT JOIN vw_Supplier s ON d.Sap_Code COLLATE Latin1_General_CI_AI = s.fldCode COLLATE Latin1_General_CI_AI AND m.fldBankCode COLLATE Latin1_General_CI_AI = s.fldBankCode COLLATE Latin1_General_CI_AI
WHERE m.fldNo = 'M.1/OP-TREA/GPI-TBS/VIII/2026'

SELECT * FROM [CentralAuthentication].dbo.tblManPower AS c
SELECT * FROM vw_Supplier WHERE fldCode='1001011980'

SELECT * FROM vw_Supplier WHERE fldName LIKE '%%'

SELECT b.fldBankCode,b.fldBankAccountNo,b.fldNameOnBankAccount,b.fldBankName,b.fldBankAddress1,b.fldBankAddress2,b.fldSKN
FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER] AS a
JOIN vw_Supplier AS b ON a.Sap_Code=b.fldCode
WHERE a.SUPPLIER_CODE='EMKJP' AND a.Client_ID='M445' AND a.SUPP_TYPE=2 AND a.Sap_Code <> '' AND uActive='Y'

USE OCPPaymentSystem
SELECT * FROM tbdMemo


