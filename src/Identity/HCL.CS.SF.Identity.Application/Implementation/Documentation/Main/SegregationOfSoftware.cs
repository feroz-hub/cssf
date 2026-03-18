/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page SegregationOfSoftware Segregation of software
* <p>The .NET HCL.CS.SF follows &ldquo; <strong> Onion Architecture </strong> &rdquo; with <strong> SOLID </strong> principles, it is based on the&nbsp;inversion of control&nbsp;principle. Onion Architecture is comprised of multiple concentric layers interfacing each other towards the core that represents the domain. It applies the fundamental rule by moving all coupling towards the center of the domain model, which represents the business and behavior objects. Around the domain layer are other layers, with more behaviors.</p>
* \image html OnionArchitecture.jpg
* <p><center><strong> HCL.CS.SF: Segregation of Software </strong></center></p>
* <p>HCL.CS.SF architecture follows Onion Architecture and consists of the following layers:</p>
* <p><strong> Domain </strong></p>
* <p>Domain layer is a center part of the HCL.CS.SF architecture, it holds all application domain entities which are database models created by Code First approach. These domain entities don&rsquo;t have any dependencies and flat as they should be, without any heavy code or dependencies. All application specific domain entities such as Users, Roles, UserClaims, UserRoles, UserPermissions etc. are maintained in this layer, and they inherit from a base entity. The domain entities are mapped to the underlying datastore schema using an object relational mapping model.</p>
* <p><strong> Domain Services </strong></p>
* <p>Domain services layer implement repository design pattern, this layer acts as a middle layer between the domain entities and business logic of an application. In this layer, we typically add interfaces that provide object saving and retrieving behavior typically by involving a database. This layer consists of the data access pattern, which is a more loosely coupled approach to data access. Also create a generic repository, and add queries to retrieve data from the source, map the data from data source to a business entity, and persist changes in the business entity to the data source.</p>
* <p><strong> Service Interfaces </strong></p>
* <p>Service layer holds interfaces with common operations, such as Add, Save, Update, and Delete. Also, this layer is used to communicate between the UI /application layer and repository layer. The Service layer also hold business logic for an entity. In this layer, service interfaces are kept separate from its implementation, keeping loose coupling and separation of concerns</p>
* <p><strong> Application Services </strong></p>
* <p>This layer is the bridge between external infrastructure and the domain layers. This layer implements application specific business logic/use cases make calls to the Domain Services and Domain Entities and Infrastructure Services in order to execute the specified business operation. For example, UserService is an application service. UserService provide functionalities to check for authentication for user and authorization for particular resource, change user permission for a user by admin. To accomplish these use cases, it would use UserRepository and UserEntity from lower layers.</p>
* <p><strong> Infrastructure Layer </strong></p>
* <p>Infrastructure layer is the outermost layer which deals with Infrastructure needs and provides the implementation of repositories interfaces. This layer also includes implementation of data access logic or logging logic or service calls logic. Only the infrastructure layer knows about the database and data access technology (Entity framework) and other layers don&rsquo;t know anything about from where the data comes and how it is being stored.</p>
* <ul>
* <li><strong> Infrastructure Data: </strong> It consists of Language-Integrated Query (LINQ) queries which performs CRUD operations for retrieving data through EntityDBContext from underlying datastore.</li>
* <li><strong> Resources: </strong> It consist of resource files which hold validation messages and constants used everywhere. It also holds the configuration settings, token settings that are required for the HCL.CS.SF.</li>
* <li><strong> Services: </strong> It contains the implementation for Logger, Audit Trail and Notification Services. These components will be used within HCL.CS.SF as well target user applications.
* <ul>
* <li><strong> Logger: </strong> It is developed using Serilog, it will be used across HCL.CS.SF. This module will log all user or system activities in file/database based on the configuration. Log file size and location are controlled through logger configuration parameters.</li>
* <li><strong> Audit Trail: </strong> It capture the key information of user/system activities along with old and new state, that will be stored in the local data store. This component will be used to track all user related account changes, roles etc these data will be used to investigate the security incidents or violations.</li>
* <li><strong> Notifications: </strong> It is used to send an email and SMS notifications from HCL.CS.SF. Notification can be triggered by framework (scenarios such account creation, account verification etc.) and invoked by target application based on business requirements. It supports sending notification based on predefined/custom template.</li>
* </ul>
* </li>
* </ul>
*/


