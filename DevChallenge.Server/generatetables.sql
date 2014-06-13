--drop table AgentRecord
--drop table [User]
--drop table Instance
--drop table Scenario


create table Scenario (
	Id integer primary key identity(1,1),
	Name nvarchar(256) not null,
	Description NText,
	Code NText
)

create table Instance (
	Id integer primary key identity(1,1),
	ScenarioId int not null,
	Started datetime,
	Finished datetime,
	Data NText,
	foreign key (ScenarioId) references Scenario(Id)
)

create table [User] (
	Id integer primary key identity(1,1),
	Login nvarchar(50) not null,
	PwDigest nvarchar(1024),
	Salt nvarchar(1024),
	FullName nvarchar(256),
	Email nvarchar(256)
)

create table AgentRecord
(
	Id integer primary key identity(1,1),
	InstanceId integer not null,
	Name nvarchar(256) not null,
	Revision integer not null,
	UserId int not null,
	Score int,
	foreign key (InstanceId) references Instance(Id),
	foreign key (UserId) references [User](Id)

)