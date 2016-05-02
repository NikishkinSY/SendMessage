# SendMessage

This service helps connected AT-modems and SMTP-clients to send SMS and Email respectively via WCF-service.

Main features:
- You can send a big amount of messages in parallel mode.
- Any crash of app saves its state and after reboot continue the task.
- AT-modems pinged constantly. That is why if modem in bad state it will never used until will be ready.
- All history saved in database.

Techs: NHibernate, Firebird, WCF
SerialPort, AT-commands, SMTP-protocol, Quartz, Automapper
