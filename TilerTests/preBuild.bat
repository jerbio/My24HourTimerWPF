

rem if this bat script gets longer than 16 lines then its time to switch to node

IF EXIST test-db.mdf (
del test-db.mdf
) ELSE (
echo test-db.mdf does not exist
)

IF EXIST test-db_log.ldf (
del test-db_log.ldf
) ELSE (
echo test-db_log.ldf does not exist
)