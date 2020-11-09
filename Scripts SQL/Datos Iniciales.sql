insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Mi SAT', 'SAT', 'CONFIGURATION', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Roles', 'Role', 'CONFIGURATION', GETDATE(), GETDATE(), 'ACTIVE', 1, 'BOTH')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Usuarios', 'User', 'CONFIGURATION', GETDATE(), GETDATE(), 'ACTIVE', 1, 'BOTH')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Cuentas', 'Account', 'CONFIGURATION', GETDATE(), GETDATE(), 'ACTIVE', 0, 'BOTH')


insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Diagnóstico', 'Diagnostic', 'REPORTS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Clientes', 'Customer', 'CUSTOMERS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Proveedores', 'Provider', 'PROVIDERS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Banco', 'Bank', 'BANKS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')


insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Planes', 'Plan', 'PLANS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_BACK_OFFICE')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Alianzas y Descuentos', 'Alliance', 'ALLIANCES_DISCOUNTS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_BACK_OFFICE')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Promociones', 'Promotion', 'ALLIANCES_DISCOUNTS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_BACK_OFFICE')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Regularizaciones', 'Quotation', 'QUOTATION', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_BACK_OFFICE')


insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Facturación', 'Invoicing', 'INVOICING', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')


insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Sucursales', 'BranchOffice', 'CONFIGURATION', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')


insert into roles
(uuid, name, code, description, createdAt, modifiedAt, status)
values(NEWID(), 'Account Owner', 'ACCOUNT_OWNER', 'ACCOUNT_OWNER', GETDATE(), GETDATE(), 'ACTIVE')


insert into roles
(uuid, name, code, description, createdAt, modifiedAt, status)
values(NEWID(), 'Lead', 'LEAD', 'LEAD', GETDATE(), GETDATE(), 'ACTIVE')


insert into roles
(uuid, name, code, description, createdAt, modifiedAt, status)
values(NEWID(), 'System Administrator', 'SYSTEM_ADMINISTRATOR', 'System Administrator', GETDATE(), GETDATE(), 'ACTIVE')


insert into rolesPermissions
(level, roleId, permissionId)
select 'FULL_ACCESS', 1, id from permissions
where applyTo<>'ONLY_BACK_OFFICE'

insert into rolesPermissions
(level, roleId, permissionId)
values ('FULL_ACCESS', 2, 4)

insert into rolesPermissions
(level, roleId, permissionId)
select 'FULL_ACCESS', 3, id from permissions

/*insert into users
(uuid, name, password, createdAt, modifiedAt, status, isBackOffice, profileId)
values (newid(), 'admin@admin.com', '3b612c75a7b5048a435fb6ec81e52ff92d6d795a8b5a9c17070f6a63c97a53b2', getdate(), getdate(), 'ACTIVE', 1,1)

insert into memberships
(userId, roleId,status)
values(19, 9, 'ACTIVE')

insert into rolesPermissions
(level, roleId, permissionId)
select 'FULL_ACCESS', 9, id from permissions*/

insert into planChargeConfigurations
(uuid, charge, createdAt, modifiedAt, status, planId, chargeId)
values(NEWID(), 20, GETDATE(), GETDATE(), 'ACTIVE', 1, 3)

insert into plans
(uuid, name, isCurrent, createdAt, modifiedAt, status)
values(NEWID(), 'Contigo', 1, GETDATE(), GETDATE(), 'ACTIVE')

insert into planCharges
(uuid, name, chargeType, createdAt, modifiedAt, status)
values(NEWID(), 'Contabilidad Mensual', 'FIXED', GETDATE(), GETDATE(), 'ACTIVE')


insert into planCharges
(uuid, name, chargeType, createdAt, modifiedAt, status, resourceTable, resourceField, operation)
values(NEWID(), 'Cargo por movimiento', 'VARIABLE', GETDATE(), GETDATE(), 'ACTIVE', 'movements', 'amount', 'MULTIPLICITY')

insert into planAssignments
(uuid, name, chargeType, resourceTable, resourceField, operation,dataType, fielType, providerData, createdAt, modifiedAt, status)
values(NEWID(), 'Rango de movimientos', 'VARIABLE', 'movements', 'amount', 'RANGE', 'System.Int32', 'text', null, GETDATE(), GETDATE(), 'ACTIVE')

insert into planAssignments
(uuid, name, chargeType, resourceTable, resourceField, operation,dataType, fielType, providerData, createdAt, modifiedAt, status)
values(NEWID(), 'Tipo de persona', 'VARIABLE', 'account', 'type', 'EQUAL', 'System.Int32', 'select', 'PersonType', GETDATE(), GETDATE(), 'ACTIVE')
