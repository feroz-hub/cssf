/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page APIList API List
* <p>The following list of APIs are supported in the HCL.CS.SF. It includes secure and anonymous APIs.</p>
* <p>All secure APIs require token to verify the permissions. The HCL.CS.SF has pre-defined set of permissions for all modules. These permissions are to be configured against user to provide appropriate level of access. </p>
* <p>The permissions are classifed as four types as follows</p>
* <ul>
* <li>Manage - It is the highest permission, the user can perform all operations including Read, Write/Update and Delete in the specific module.</li>
* <li>Delete - This permission allows to delete records in the specific module.</li>
* <li>Write/Update - This permission allow to create or update records in the specific module</li>
* <li>Read - This permission type allows to read data from the system in the specific module</li>
* </ul>
* <p><strong>Note:&nbsp;</strong> When assigning permission to user role, module name should be appended as a prefix like &lt;SystemName&gt;.&lt;ModuleName&gt;.&lt;Permission&gt;</p>
* <p>Example, "HCL.CS.SF.Role.manage", "HCL.CS.SF.Role.read", "HCL.CS.SF.Role.write", "HCL.CS.SF.Role.delete"</p>
* <table>
* <tr>
* <td>
* <p><strong> # </strong></p>
* </td>
* <td>
* <p><strong> HCL.CS.SF API list </strong></p>
* </td>
* <td>
* <p><strong> Is Secure Api ? </strong></p>
* </td>
* <td>
* <p><strong> Is Admin Api ? </strong></p>
* </td>
* <td>
* <p><strong> Is General user Api ? </strong></p>
* </td>
* <td>
* <p><strong> Permission needed to access Api </strong></p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> Api Resource </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>1</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddApiResourceAsync(ApiResourcesModel apiResourceModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>2</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; UpdateApiResourceAsync(ApiResourcesModel apiResourceModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>3</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteApiResourceAsync(Guid apiResourceId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>4</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteApiResourceAsync(string apiResourceName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>5</p>
* </td>
* <td>
* <p>Task&lt;ApiResourcesModel&gt; GetApiResourceAsync(string apiResourceName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>6</p>
* </td>
* <td>
* <p>Task&lt;ApiResourcesModel&gt; GetApiResourceAsync(Guid apiResourceId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>7</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;ApiResourcesByScopesModel&gt;&gt; GetAllApiResourcesByScopesAsync(IList&lt;string&gt; requestedScopes)</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>8</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;ApiResourcesModel&gt;&gt; GetAllApiResourcesAsync();</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>9</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;ApiScopesModel&gt;&gt; GetAllApiScopesAsync();</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>10</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddApiResourceClaimAsync(ApiResourceClaimsModel apiResourceClaimModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>11</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteApiResourceClaimAsync(Guid apiResourceId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>12</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteApiResourceClaimAsync(ApiResourceClaimsModel apiResourceClaimModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>13</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;ApiResourceClaimsModel&gt;&gt; GetApiResourceClaimsAsync(Guid apiResourceId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>14</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddApiScopeAsync(ApiScopesModel apiScopesModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>15</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; UpdateApiScopeAsync(ApiScopesModel apiScopesModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>16</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteApiScopeAsync(Guid apiScopeId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>17</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteApiScopeAsync(string apiScopeName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>18</p>
* </td>
* <td>
* <p>Task&lt;ApiScopesModel&gt; GetApiScopeAsync(Guid apiScopeId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>19</p>
* </td>
* <td>
* <p>Task&lt;ApiScopesModel&gt; GetApiScopeAsync(string apiScopeName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>20</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddApiScopeClaimAsync(ApiScopeClaimsModel apiScopeClaimModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>21</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteApiScopeClaimAsync(Guid apiScopeId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>22</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteApiScopeClaimAsync(ApiScopeClaimsModel apiScopeClaimModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>23</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;ApiScopeClaimsModel&gt;&gt; GetApiScopeClaimsAsync(Guid apiScopeId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.ApiResource.manage/ <br /> HCL.CS.SF.ApiResource.read</p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> Identity Resource </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>24</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddIdentityResourceAsync(IdentityResourcesModel identityResourceModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>25</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; UpdateIdentityResourceAsync(IdentityResourcesModel identityResourceModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>26</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteIdentityResourceAsync(Guid identityResourceId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>27</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteIdentityResourceAsync(string identityResourceName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>28</p>
* </td>
* <td>
* <p>Task&lt;IdentityResourcesModel&gt; GetIdentityResourceAsync(Guid identityResourceId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>29</p>
* </td>
* <td>
* <p>Task&lt;IdentityResourcesModel&gt; GetIdentityResourceAsync(string identityResourceName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>30</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;IdentityResourcesModel&gt;&gt; GetAllIdentityResourcesAsync();</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>31</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddIdentityResourceClaimAsync(IdentityClaimsModel identityClaimsModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>32</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteIdentityResourceClaimAsync(Guid identityResourceId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>33</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteIdentityResourceClaimAsync(IdentityClaimsModel identityClaimsModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>34</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;IdentityResourcesByScopesModel&gt;&gt; GetAllIdentityResourcesByScopesAsync(IList&lt;string&gt; requestedScopes);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>35</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;IdentityClaimsModel&gt;&gt; GetIdentityResourceClaimsAsync(Guid identityResourceId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.IdentityResource.manage/ <br /> HCL.CS.SF.IdentityResource.read</p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> Client Service </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>36</p>
* </td>
* <td>
* <p>Task&lt;ClientsModel&gt; RegisterClientAsync(ClientsModel clientsModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Client.manage/ <br /> HCL.CS.SF.Client.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>37</p>
* </td>
* <td>
* <p>Task&lt;ClientsModel&gt; UpdateClientAsync(ClientsModel clientsModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Client.manage/ <br /> HCL.CS.SF.Client.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>38</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteClientAsync(string clientId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Client.manage/ <br /> HCL.CS.SF.Client.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>39</p>
* </td>
* <td>
* <p>Task&lt;ClientsModel&gt; GenerateClientSecret(string clientId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Client.manage/ <br /> HCL.CS.SF.Client.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>40</p>
* </td>
* <td>
* <p>Task&lt;ClientsModel&gt; GetClientAsync(string clientId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Client.manage/ <br /> HCL.CS.SF.Client.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>41</p>
* </td>
* <td>
* <p>Task&lt;Dictionary&lt;string, string&gt;&gt; GetAllClientAsync();</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Client.manage/ <br /> HCL.CS.SF.Client.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>42</p>
* </td>
* <td>
* <p>Task&lt;List&lt;ClientTokenModel&gt;&gt; GetClientsActiveTokensAsync(List&lt;string&gt; clientIds);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Client.manage/ <br /> HCL.CS.SF.Client.read</p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> Audit Service </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>43</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddAuditTrailAsync(IEnumerable&lt;AuditTrailModel&gt; audits);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>44</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddAuditTrailAsync(AuditTrailModel audit);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>45</p>
* </td>
* <td>
* <p>Task&lt;AuditResponseModel&gt; GetAuditDetailsAsync(DateTime? createdOn, PagingModel page);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AuditTrail.manage/ <br /> HCL.CS.SF.AuditTrail.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>46</p>
* </td>
* <td>
* <p>Task&lt;AuditResponseModel&gt; GetAuditDetailsAsync(string createdBy, DateTime? createdOn, PagingModel page);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AuditTrail.manage/ <br /> HCL.CS.SF.AuditTrail.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>47</p>
* </td>
* <td>
* <p>Task&lt;AuditResponseModel&gt; GetAuditDetailsAsync(string createdBy, DateTime? fromDate, DateTime? toDate, PagingModel page);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AuditTrail.manage/ <br /> HCL.CS.SF.AuditTrail.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>48</p>
* </td>
* <td>
* <p>Task&lt;AuditResponseModel&gt; GetAuditDetailsAsync(string createdBy, AuditType actionType, DateTime? fromDate, DateTime? toDate, PagingModel page);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AuditTrail.manage/ <br /> HCL.CS.SF.AuditTrail.read</p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> Role Service </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>49</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; CreateRoleAsync(RoleModel roleModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Role.manage/ <br /> HCL.CS.SF.Role.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>50</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; UpdateRoleAsync(RoleModel roleModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Role.manage/ <br /> HCL.CS.SF.Role.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>51</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteRoleAsync(Guid roleId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Role.manage/ <br /> HCL.CS.SF.Role.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>52</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteRoleAsync(string roleName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Role.manage/ <br /> HCL.CS.SF.Role.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>53</p>
* </td>
* <td>
* <p>Task&lt;RoleModel&gt; GetRoleAsync(Guid roleId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.Role.manage/ <br /> HCL.CS.SF.Role.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>54</p>
* </td>
* <td>
* <p>Task&lt;RoleModel&gt; GetRoleAsync(string roleName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.Role.manage/ <br /> HCL.CS.SF.Role.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>55</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddRoleClaimAsync(RoleClaimModel roleClaimModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Role.manage/ <br /> HCL.CS.SF.Role.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>56</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddRoleClaimsAsync(IList&lt;RoleClaimModel&gt; roleClaimsModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Role.manage/ <br /> HCL.CS.SF.Role.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>57</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; RemoveRoleClaimAsync(RoleClaimModel roleClaimModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Role.manage/ <br /> HCL.CS.SF.Role.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>58</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; RemoveRoleClaimsAsync(IList&lt;RoleClaimModel&gt; roleClaimsModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.Role.manage/ <br /> HCL.CS.SF.Role.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>59</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;Claim&gt;&gt; GetRoleClaimAsync(RoleModel roleModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.Role.manage/ <br /> HCL.CS.SF.Role.read</p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> User Account Service </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>60</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; RegisterUserAsync(UserModel user);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>61</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; UpdateUserAsync(UserModel userModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>62</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteUserAsync(string username);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>63</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteUserAsync(Guid userId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>64</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>NO</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>65</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; ResetPasswordAsync(Guid userId, string passwordResetToken, string newPassword);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>66</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; LockUserAsync(Guid userId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>67</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; LockUserAsync(Guid userId, DateTime? dateTime);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>68</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; UnLockUserAsync(Guid userId);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>69</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; UnLockUserAsync(Guid userId, string token, string purpose);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>70</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; UnlockUserAsync(Guid userId, IList&lt;UserSecurityQuestionModel&gt; userSecurityQuestions);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>71</p>
* </td>
* <td>
* <p>Task&lt;UserModel&gt; GetUserByNameAsync(string userName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>72</p>
* </td>
* <td>
* <p>Task&lt;UserModel&gt; GetUserByEmailAsync(string email);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>73</p>
* </td>
* <td>
* <p>Task&lt;UserModel&gt; GetUserByIdAsync(Guid userId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>74</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;UserModel&gt;&gt; GetUsersForClaimAsync(Claim claim);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>75</p>
* </td>
* <td>
* <p>Task&lt;bool&gt; IsUserExistsAsync(ClaimsPrincipal claimsPrincipal);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>76</p>
* </td>
* <td>
* <p>Task&lt;bool&gt; IsUserExistsAsync(Guid userId);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>77</p>
* </td>
* <td>
* <p>Task&lt;bool&gt; IsUserExistsAsync(string userName);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> User Claims </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>78</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddClaimAsync(UserClaimModel userClaimModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>79</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddClaimAsync(IList&lt;UserClaimModel&gt; userClaimModels);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>80</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; RemoveClaimAsync(UserClaimModel userClaimModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>81</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; RemoveClaimAsync(IList&lt;UserClaimModel&gt; userClaimModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>82</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; ReplaceClaimAsync(UserClaimModel existingUserClaimModel, UserClaimModel newUserClaimModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>83</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;Claim&gt;&gt; GetClaimsAsync(Guid userId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>84</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;UserClaimModel&gt;&gt; GetUserClaimsAsync(Guid userId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>85</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddAdminClaimAsync(IList&lt;UserClaimModel&gt; userClaimModels)</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>86</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; RemoveAdminClaimAsync(UserClaimModel userClaimModel)</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>87</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; RemoveAdminClaimAsync(IList&lt;UserClaimModel&gt; userClaimModel)</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>88</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddAdminClaimAsync(UserClaimModel userClaimModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.write</p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> User Roles </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>89</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddUserRoleAsync(UserRoleModel userRoleModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>90</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddUserRolesAsync(IList&lt;UserRoleModel&gt; modelList);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>91</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; RemoveUserRoleAsync(UserRoleModel userRoleModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>92</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; RemoveUserRolesAsync(IList&lt;UserRoleModel&gt; modelList);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>93</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;string&gt;&gt; GetUserRolesAsync(Guid userId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>94</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;UserModel&gt;&gt; GetUsersInRoleAsync(string roleName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>95</p>
* </td>
* <td>
* <p>Task&lt;UserPermissionsResponseModel&gt; GetUserRoleClaimsByIdAsync(Guid userId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.read</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>96</p>
* </td>
* <td>
* <p>Task&lt;UserPermissionsResponseModel&gt; GetUserRoleClaimsByNameAsync(string userName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p><br /> HCL.CS.SF.User.read</p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> Security Questions </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>97</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddSecurityQuestionAsync(SecurityQuestionModel securityQuestionModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>98</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; UpdateSecurityQuestionAsync(SecurityQuestionModel securityQuestionModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>99</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteSecurityQuestionAsync(Guid securityQuestionId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>HCL.CS.SF.AdminUser.manage/ <br /> HCL.CS.SF.AdminUser.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>100</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;SecurityQuestionModel&gt;&gt; GetAllSecurityQuestionsAsync();</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>101</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddUserSecurityQuestionAsync(UserSecurityQuestionModel userSecurityQuestionModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>102</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; AddUserSecurityQuestionAsync(IList&lt;UserSecurityQuestionModel&gt; userSecurityQuestionModels);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>103</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; UpdateUserSecurityQuestionAsync(UserSecurityQuestionModel userSecurityQuestionModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>104</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteUserSecurityQuestionAsync(UserSecurityQuestionModel userSecurityQuestionModel);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>105</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; DeleteUserSecurityQuestionAsync(IList&lt;UserSecurityQuestionModel&gt; userSecurityQuestionModels);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.delete</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>106</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;UserSecurityQuestionModel&gt;&gt; GetUserSecurityQuestionsAsync(Guid userId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.read</p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> User Token </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>107</p>
* </td>
* <td>
* <p>Task&lt;string&gt; GenerateEmailConfirmationTokenAsync(Guid userId);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>108</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; GenerateAndSendEmailConfirmationTokenAsync(Guid userId);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>109</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; VerifyEmailConfirmationTokenAsync(Guid userId, string emailToken);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>110</p>
* </td>
* <td>
* <p>Task&lt;string&gt; GeneratePhoneNumberConfirmationTokenAsync(Guid userId);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>111</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; GenerateAndSendPhoneNumberConfirmationTokenAsync(Guid userId);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>112</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; VerifyPhoneNumberConfirmationTokenAsync(Guid userId, string smsToken);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>113</p>
* </td>
* <td>
* <p>Task&lt;string&gt; GeneratePasswordResetTokenAsync(Guid userId);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>114</p>
* </td>
* <td>
* <p>Task&lt;string&gt; GenerateUserTokenAsync(Guid userId, string purpose);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>115</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; VerifyUserTokenAsync(Guid userId, string purpose, string token);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>116</p>
* </td>
* <td>
* <p>Task&lt;string&gt; GenerateEmailTwoFactorTokenAsync(Guid userId);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>117</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; VerifyEmailTwoFactorTokenAsync(Guid userId, string emailToken);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>118</p>
* </td>
* <td>
* <p>Task&lt;string&gt; GenerateSmsTwoFactorTokenAsync(Guid userId);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>119</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; VerifySmsTwoFactorTokenAsync(Guid userId, string smsToken);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> Two Factor </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>120</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; SetTwoFactorEnabledAsync(Guid userId, bool enabled);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>121</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; UpdateUserTwoFactorTypeAsync(Guid userId, TwoFactorType twoFactorType);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>122</p>
* </td>
* <td>
* <p>Task&lt;IList&lt;string&gt;&gt; GetAllTwoFactorTypeAsync();</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td colspan="6">
* <p><strong> Authentication Service </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>123</p>
* </td>
* <td>
* <p>Task&lt;SignInResponseModel&gt; PasswordSignInAsync(string username, string password);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>124</p>
* </td>
* <td>
* <p>Task&lt;SignInResponseModel&gt; PasswordSignInAsync(string username, string password, string twoFactorAuthenticatorToken);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>125</p>
* </td>
* <td>
* <p>Task&lt;SignInResponseModel&gt; TwoFactorEmailSignInAsync(string code);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>126</p>
* </td>
* <td>
* <p>Task&lt;SignInResponseModel&gt; TwoFactorSmsSignInAsync(string code);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>127</p>
* </td>
* <td>
* <p>Task&lt;SignInResponseModel&gt; TwoFactorAuthenticatorAppSignInAsync(string code);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>128</p>
* </td>
* <td>
* <p>Task&lt;SignInResponseModel&gt; TwoFactorRecoveryCodeSignInAsync(string recoveryCode);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>129</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; SignOutAsync();</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>130</p>
* </td>
* <td>
* <p>Task&lt;bool&gt; IsUserSignedInAsync(ClaimsPrincipal principal);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>131</p>
* </td>
* <td>
* <p>Task&lt;AuthenticatorAppSetupResponseModel&gt; SetupAuthenticatorAppAsync(Guid userId, string applicationName);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>132</p>
* </td>
* <td>
* <p>Task&lt;AuthenticatorAppResponseModel&gt; VerifyAuthenticatorAppSetupAsync(Guid userId, string token);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>133</p>
* </td>
* <td>
* <p>Task&lt;FrameworkResult&gt; ResetAuthenticatorAppAsync(Guid userId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>134</p>
* </td>
* <td>
* <p>Task&lt;IEnumerable&lt;string&gt;&gt; GenerateRecoveryCodesAsync(Guid userId);</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>Yes</p>
* </td>
* <td>
* <p>HCL.CS.SF.User.write</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>135</p>
* </td>
* <td>
* <p>Task&lt;RopValidationModel&gt; RopValidateCredentialsAsync(RopValidationModel validationModel);</p>
* </td>
* <td>
* <p>No</p>
* </td>
* <td>&nbsp;</td>
* <td>&nbsp;</td>
* <td>
* <p>Anonymous</p>
* </td>
* </tr>
* </table>
*/


