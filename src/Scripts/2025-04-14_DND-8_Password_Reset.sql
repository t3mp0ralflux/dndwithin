-- DND-8: Password Reset

begin transaction;

alter table account
add password_reset_requested_utc timestamp null;

alter table account
add password_reset_code varchar null;

commit;