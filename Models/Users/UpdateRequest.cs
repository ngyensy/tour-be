using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic;
using WebApi.Entities;

namespace WebApi.Models.Users
{
    public class UpdateRequest
    {
        public string Title { get; set; }
        public string Avatar {get; set;}
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }

        [EnumDataType(typeof(Role))]
        public string Role { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        // treat empty string as null for password fields to 
        // make them optional in front end apps
        private string _password;
        [MinLength(6)]
        public string Password
        {
            get => _password;
            set => _password = replaceEmptyWithNull(value);
        }

        private string _confirmPassword;
        [Compare("Password")]
        public string ConfirmPassword 
        {
            get => _confirmPassword;
            set => _confirmPassword = replaceEmptyWithNull(value);
        }

        // thêm thuộc tính ngày sinh
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        // helpers
        private string replaceEmptyWithNull(string value)
        {
            // replace empty string with null to make field optional
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
