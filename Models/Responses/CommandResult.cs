﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Responses
{
    public class CommandResult
    {
        public bool IsSuccessful { get; set; }
        public bool NotFound { get; set; }
        public bool UnknownInternalError { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public object CreatedId { get; set; }


        public static readonly CommandResult SuccessResult = new() { IsSuccessful = true };
        public static readonly CommandResult UnknownInternalErrorResult = new()
        {
            IsSuccessful = false,
            UnknownInternalError = true,
            Errors = new List<string> { "Unknown error occured." }
        };

        public static CommandResult GetErrorResult(string error)
        {
            return new CommandResult()
            {
                IsSuccessful = false,
                Errors = new List<string> { error }
            };
        }
        public static CommandResult GetErrorResult(IEnumerable<string> errors)
        {
            return new CommandResult()
            {
                IsSuccessful = false,
                Errors = errors
            };
        }

        public static CommandResult GetNotFoundResult(string error)
        {
            return new CommandResult()
            {
                IsSuccessful = false,
                NotFound = true,
                Errors = new List<string> { error }
            };
        }

        public static CommandResult GetNotFoundResult(IEnumerable<string> errors)
        {
            return new CommandResult()
            {
                IsSuccessful = false,
                NotFound = true,
                Errors = errors
            };
        }

        public static CommandResult GetCreatedResult(object createdId)
        {
            return new CommandResult()
            {
                IsSuccessful = true,
                CreatedId = createdId
            };
        }
    }
}
