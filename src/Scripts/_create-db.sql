create table if not exists account (
    id UUID primary key,
    first_name varchar(25) not null,
    last_name varchar(25) not null,
    username varchar(25) not null,
    password varchar not null,
    email varchar(50) not null,
    created_utc timestamp not null,
    updated_utc timestamp not null,
    activated_utc timestamp null,
    last_login_utc timestamp not null,
    deleted_utc timestamp null,
    account_status int not null,
    account_role int not null,
	UNIQUE (username, email)
);

create table if not exists character(
    id UUID primary key,
    account_id UUID references account(id),
    name varchar(100) not null,
    username varchar(25),
    created_utc timestamp not null,
    updated_utc timestamp not null,
    deleted_utc timestamp null
);

create table if not exists characteristics(
    id UUID primary key,
    character_id UUID references character(id),
    gender varchar(100) not null,
    age varchar(100) not null,
    hair varchar(100) not null,
    eyes varchar(100) not null,
    skin varchar(100) not null,
    height varchar(100) not null,
    weight varchar(100) not null,
    faith varchar(100) not null,
    UNIQUE(character_id)
);

create table if not exists email(
    id UUID primary key,
    account_id_sender UUID references account(id),
    account_id_receiver UUID references account(id),
    should_send boolean not null,
    send_attempts int not null,
    sent_utc timestamp null,
    send_after_utc timestamp not null,
    sender_email varchar(50) not null,
    recipient_email varchar(50) not null,   
    body varchar not null,
    response_log varchar null
);

create table if not exists globalsettings (
    id UUID primary key,
    name varchar not null,
    value varchar not null
);