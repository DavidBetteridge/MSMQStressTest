# MSMQStressTest
Checks the behaviour of a MSMQ transaction using snapshot isolation when under load

## Setup
1. Create a transactional MSMQ message queue called StressTest
2. Run the database script in order to create your test sql server database
3. Check the app.config settings are correct in both the publisher and subscribers
4. Start the subscriber running,  click the big button.  Check that there is no error file in the application's folder.
5. Start the publisher running
6. After about 1000 or so transactions, you normally see an error logged.