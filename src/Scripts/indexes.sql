create unique index concurrently if not exists account_username_idx
on account
using btree(username);