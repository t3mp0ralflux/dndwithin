create table if not exists account (
    id UUID primary key,
    first_name varchar(25) not null,
    last_name varchar(25) not null,
    username varchar(25) not null,
    email varchar(50) not null,
    mobile_phone varchar(30) not null,
    created_utc timestamp not null,
    updated_utc timestamp not null,
    last_login_utc timestamp not null,
    deleted_utc timestamp null,
    account_status int not null,
    account_role int not null);