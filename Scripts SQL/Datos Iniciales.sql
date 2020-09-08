insert into roles
(code, name, uuid, description, createdAt, modifiedAt, status)
values ('SYSTEM_ADMINISTRATOR', 'System Administrator', newid(), 'System Administrator',getdate(),getdate(), 'ACTIVE')

insert into users
(uuid, name, password, createdAt, modifiedAt, status, isBackOffice, profileId)
values (newid(), 'admin@admin.com', '3b612c75a7b5048a435fb6ec81e52ff92d6d795a8b5a9c17070f6a63c97a53b2', getdate(), getdate(), 'ACTIVE', 1,1)

insert into memberships
(userId, roleId,status)
values(19, 9, 'ACTIVE')

insert into rolesPermissions
(level, roleId, permissionId)
select 'FULL_ACCESS', 9, id from permissions



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
