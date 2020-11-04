delete from credentials
delete from quotationDetails
delete from quotations
delete from branchOffices
delete from memberships
delete from rolesPermissions where accountId is not null
delete from roles where accountId is not null
delete from invoicesIssued
delete from invoicesReceived
delete from customersContacts
delete from customers
delete from providersContacts
delete from providers
delete from diagnosticDetails
delete from diagnosticTaxStatus
delete from diagnostics
delete from bankTransactions
delete from bankAccounts
delete from bankCredentials
delete from discounts
delete from promotionAccounts
delete from accounts