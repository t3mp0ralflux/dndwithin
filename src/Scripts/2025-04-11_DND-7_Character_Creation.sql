-- DND-7: Character Creation

begin transaction;

drop table if exists accountactivation;

alter table account
add activation_code varchar(50) null;

alter table account
add activation_expiration timestamp null;

commit;