# templater
This project is based on Jint to help making templates. It can be mail templates or anything else you might want to template. Since it is based on Jint you can use full javascript code within you templates

Based on an existing project developped by Sébastien Ros, this template engine has been externalized to be used in other projects.

You can use template like this :
```
Dear <%= user.FirstName %> <%= user.LastName %>,

Your password has been resetted on your request.

Your new password is <%= newPassword %>.

Now you can connect your account using the link below:
<%= loginUrl %>
When connected, you can change your password and modify your personal information.
You don’t see the advantage ? Have a closer look : user is a .NET object injected using the SetParameter of Jint. Therefore, you can provide an API to business people in order they can write their own template with their own logic. Pretty useful, isn’t it ?
```
You need a more complex template ? ok, here is another one :
```
Hello,



You have received a mail from <%= firstName %> <%= lastName %>

<% if (organisation)
{
    write(" from ");
    write(organisation);
}%>


<% if (contactType)
{
    write(" Type: ");
    write(contactType);
}%>


<%if (phoneNumber)
{
    write("You can call him/her at this number : ");
    write(phoneNumber);
}
%>





<%= message %>
```


##Special Thanks

I would like to thank @sebastienros who originally had the idea.
