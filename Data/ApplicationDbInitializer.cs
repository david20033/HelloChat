using System.Data.Entity;
using HelloChat.Services;
using HelloChat.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;

namespace HelloChat.Data
{
    public class ApplicationDbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<HelloChatDbContext>();
            if (context.Users.Count() > 0) { return; }
            await context.Database.MigrateAsync();

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            List<ApplicationUser> UsersList = [];
            UsersList.Add(new ApplicationUser
            {
                FirstName = "David",
                LastName = "Dimitrov",
                UserName = "daviddimitrov123@mail.com",
                Email = "daviddimitrov123@mail.com",
                DateOfBirth = new DateTime(2003, 2, 15),
                ProfilePicturePath = "/images/blank-profile-picture.webp",
                isActive = false,
            });
            UsersList.Add(new ApplicationUser
            {
                FirstName = "Ivan",
                LastName = "Ivanov",
                UserName = "vanko10@mail.com",
                Email = "vanko10@mail.com",
                DateOfBirth = new DateTime(2001, 3, 2),
                ProfilePicturePath = "/images/blank-profile-picture.webp",
                isActive = false,
            });
            UsersList.Add(new ApplicationUser
            {
                FirstName = "Grigor",
                LastName = "Dimitrov",
                UserName = "grigor123@mail.com",
                Email = "grigor123@mail.com",
                DateOfBirth = new DateTime(2003, 2, 15),
                ProfilePicturePath = "/images/blank-profile-picture.webp",
                isActive = false,
            });
            UsersList.Add(new ApplicationUser
            {
                FirstName = "Silviq",
                LastName = "Slivova",
                UserName = "sisito@mail.com",
                Email = "sisito@mail.com",
                DateOfBirth = new DateTime(2003, 2, 15),
                ProfilePicturePath = "/images/blank-profile-picture.webp",
                isActive = false,
            });
            UsersList.Add(new ApplicationUser
            {
                FirstName = "Kaloqn",
                LastName = "Kolev",
                UserName = "koki@mail.com",
                Email = "koki@mail.com",
                DateOfBirth = new DateTime(2003, 2, 15),
                ProfilePicturePath = "/images/blank-profile-picture.webp",
                isActive = false,

            });
            string password = "123456a";
            foreach (var user in UsersList)
            {
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception("User creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            await context.SaveChangesAsync();
            var profileService = serviceProvider.GetRequiredService<IProfileService>();
            var homeService = serviceProvider.GetRequiredService<IHomeService>();
            for (int i = 0; i < UsersList.Count; i++)
            {
                for (int j = i + 1; j < UsersList.Count; j++)
                {
                    var userA = UsersList[i];
                    var userB = UsersList[j];

                    await profileService.SendFriendRequest(userA.Id, userB.Id);

                    await profileService.AcceptFriendRequest(userB.Id, userA.Id);
                }
            }
            foreach (var user in UsersList)
            {
                await homeService.GetFriendsViewModelAsync(user.Id);
            }
            var messages = new[]
{
        "Hey! How's it going?",
        "Pretty good! You?",
        "Doing great, thanks!"
    };

            for (int i = 0; i < UsersList.Count; i++)
            {
                for (int j = i + 1; j < UsersList.Count; j++)
                {
                    var userA = UsersList[i];
                    var userB = UsersList[j];

                    await homeService.SendMessageAndReturnItsId(userA.Id, userB.Id, messages[0]);

                    var id=await homeService.SendMessageAndReturnItsId(userB.Id, userA.Id, messages[1]);

                    await homeService.SendMessageAndReturnItsId(userA.Id, userB.Id, messages[2]);
                }
            }
        }
    }
}
