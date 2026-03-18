/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page HCLCSSFMiddlewareHosting HCL.CS.SF Middleware Hosting
* <p>Middleware in ASP.NET Core controls how the application responds to HTTP requests. It can also control, authenticate and authorize a user to perform specific actions.</p>
* <ul>
* <li>Middleware are software components that are assembled into an application pipeline to handle requests and responses.</li>
* <li>Can perform work before and after the next component in the pipeline.</li>
* <li>Request delegates are used to build the request pipeline. The request delegates handle each HTTP request.</li>
* <li>Request delegates are configured using&nbsp; Run,&nbsp; Map , and&nbsp; Use extension methods.</li>
* <li>Each middleware component in the request pipeline is responsible for invoking the next component in the pipeline or short-circuiting the pipeline.</li>
* <li>When a middleware short-circuits, it's called a&nbsp;terminal middleware&nbsp;because it prevents further middleware from processing the request.</li>
* </ul>
* <p>HCL.CS.SF has middleware extension which is created using <strong>IApplicationBuilder</strong>, it can be consumed in the middleware hosting. Once HCL.CS.SF middleware extension component integrated into target application, and then it is capable to handle OAuth/API request and responses via HTTPContext.</p>
*/



