﻿using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Application.Transaction;

namespace WebAPI.Controllers
{
    /**
    * @Project ASP.NET Core 7.0
    * @Author: Nguyen Xuan Nhan
    * @Team: 4FT
    * @Copyright (C) 2023 4FT. All rights reserved
    * @License MIT
    * @Create date Mon 23 Jan 2023 00:00:00 AM +07
    */

    /// <summary>
    /// Quản lý giáo viên
    /// </summary>
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ILogger<TeacherController> _logger;
        private readonly IMapper _mapper;
        private readonly TeacherService _teacherService;
        private readonly TransactionService _transactionService;

        /// <inheritdoc />
        public TeacherController(
            TeacherService teacherService,
            TransactionService transactionService,
            ILogger<TeacherController> logger)
        {
            _logger = logger;
            _mapper = new MapperConfiguration(cfg => cfg.CreateMap<Models.Teacher, Domain.Entities.Teacher>()).CreateMapper();
            _teacherService = teacherService;
            _transactionService = transactionService;
        }

        private bool IsValidTeacher(Models.Teacher teacher, out string? message)
        {
            if (string.IsNullOrWhiteSpace(teacher.fullName) ||
                string.IsNullOrWhiteSpace(teacher.email) ||
                string.IsNullOrWhiteSpace(teacher.phone))
            {
                message = "Full name, email and phone are required";
                return false;
            }

            if (!Regex.IsMatch(teacher.email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", RegexOptions.None,
                    TimeSpan.FromSeconds(2)))
            {
                message = "Email is invalid";
                return false;
            }

            if (!Regex.IsMatch(teacher.phone, @"^([0-9]{10})$", RegexOptions.None, TimeSpan.FromSeconds(2)))
            {
                message = "Phone is invalid";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(teacher.customerId) &&
                !_teacherService.GetAllId().Contains(teacher.customerId))
            {
                message = "Customer id is not exists";
                return false;
            }

            message = null;
            return true;
        }

        /// <summary>
        /// Lấy danh sách giáo viên
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/Teacher
        /// </remarks>
        /// <response code="200">Lấy danh sách giáo viên thành công</response>
        /// <response code="204">Không có giáo viên nào</response>
        /// <response code="408">Quá thời gian yêu cầu</response>
        /// <response code="429">Request quá nhiều</response>
        /// <response code="500">Lỗi server</response>
        [HttpGet()]
        [AllowAnonymous]
        [ResponseCache(Duration = 15, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "teachers" })]
        public ActionResult GetTeachers()
        {
            try
            {
                var teachers = _teacherService.GetTeachers().ToArray();
                return teachers.Any()
                    ? Ok(new { status = true, message = "Get data successfully", data = teachers })
                    : NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting all teacher");
                return StatusCode(StatusCodes.Status500InternalServerError, new { status = false, message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Lấy giáo viên theo tên
        /// </summary>
        /// <param name="name">Tên giáo viên</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/Teacher/string
        /// </remarks>
        /// <response code="200">Lấy danh sách giáo viên thành công</response>
        /// <response code="204">Không có giáo viên nào</response>
        /// <response code="408">Quá thời gian yêu cầu</response>
        /// <response code="429">Request quá nhiều</response>
        /// <response code="500">Lỗi server</response>
        [HttpGet("{name}")]
        [AllowAnonymous]
        public ActionResult GetTeachersByName([FromRoute] string name)
        {
            try
            {
                var teachers = _teacherService.FindTeacherByName(name).ToArray();
                return teachers.Any()
                    ? Ok(new { status = true, message = "Get data successfully", data = teachers })
                    : NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting teacher by name");
                return StatusCode(StatusCodes.Status500InternalServerError, new { status = false, message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Tìm kiếm giáo viên theo mã
        /// </summary>
        /// <param name="id">Mã giáo viên</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/Teacher/guid
        /// </remarks>
        /// <response code="200">Lấy giáo viên thành công</response>
        /// <response code="204">Không có giáo viên nào</response>
        /// <response code="408">Quá thời gian yêu cầu</response>
        /// <response code="429">Request quá nhiều</response>
        /// <response code="500">Lỗi server</response>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public ActionResult GetTeacherById([FromRoute] Guid id)
        {
            try
            {
                var teacher = _teacherService.GetTeacherById(id);
                return teacher != null
                    ? Ok(new { status = true, message = "Get data successfully", data = teacher })
                    : NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting teacher by id");
                return StatusCode(StatusCodes.Status500InternalServerError, new { status = false, message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Thêm giáo viên
        /// </summary>
        /// <param name="teacher">Đối tượng giáo viên</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/Teacher
        ///     {
        ///         "fullName": "string",
        ///         "email": "string",
        ///         "phone": "string",
        ///         "customerId": "uuid"
        ///     }
        /// </remarks>
        /// <response code="200">Thêm giáo viên thành công</response>
        /// <response code="400">Lỗi dữ liệu đầu vào</response>
        /// <response code="408">Quá thời gian yêu cầu</response>
        /// <response code="429">Request quá nhiều</response>
        /// <response code="500">Lỗi server</response>
        [HttpPost()]
        [AllowAnonymous]
        public ActionResult AddTeacher([FromBody] Models.Teacher teacher)
        {
            if (!IsValidTeacher(teacher, out var message))
                return BadRequest(new { status = false, message });

            try
            {
                var newTeacher = _mapper.Map<Domain.Entities.Teacher>(teacher);
                newTeacher.id = Guid.NewGuid();
                _transactionService.ExecuteTransaction(() => { _teacherService.AddTeacher(newTeacher); });
                return Ok(new { status = true, message = "Add teacher successfully" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while adding teacher");
                return StatusCode(StatusCodes.Status500InternalServerError, new { status = false, message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Cập nhật giáo viên
        /// </summary>
        /// <param name="teacher">Đối tượng giáo viên</param>
        /// <param name="id">Mã giáo viên</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/v1/Teacher/guid
        ///     {
        ///         "id": "uuid",
        ///         "fullName": "string",
        ///         "email": "string",
        ///         "phone": "string",
        ///         "customerId": "string"
        ///     }
        /// </remarks>
        /// <response code="200">Cập nhật giáo viên thành công</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="404">Không tìm thấy giáo viên</response>
        /// <response code="408">Quá thời gian yêu cầu</response>
        /// <response code="429">Request quá nhiều</response>
        /// <response code="500">Lỗi server</response>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public ActionResult UpdateTeacher([FromBody] Models.Teacher teacher, [FromRoute] Guid id)
        {
            try
            {
                if (!IsValidTeacher(teacher, out var message))
                    return BadRequest(new { status = false, message });
                var teacherToUpdate = _teacherService.GetTeacherById(id);
                if (teacherToUpdate == null)
                    return NotFound(new { status = false, message = "Teacher not found" });
                _mapper.Map(teacher, teacherToUpdate);
                _transactionService.ExecuteTransaction(() => { _teacherService.UpdateTeacher(teacherToUpdate); });
                return Ok(new { status = true, message = "Update teacher successfully" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while updating teacher");
                return StatusCode(StatusCodes.Status500InternalServerError, new { status = false, message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Xóa giáo viên
        /// </summary>
        /// <param name="id">Mã giáo viên</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/v1/Teacher/guid
        /// </remarks>
        /// <response code="200">Xóa giáo viên thành công</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="404">Không tìm thấy giáo viên</response>
        /// <response code="408">Quá thời gian yêu cầu</response>
        /// <response code="429">Request quá nhiều</response>
        /// <response code="500">Lỗi server</response>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteTeacher([FromRoute] Guid id)
        {
            try
            {
                if (_teacherService.GetTeacherById(id) == null)
                    return NotFound(new { status = false, message = "Teacher not found" });
                _transactionService.ExecuteTransaction(() => { _teacherService.DeleteTeacher(id); });
                return Ok(new { status = true, message = "Delete teacher successfully" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while deleting teacher");
                return StatusCode(StatusCodes.Status500InternalServerError, new { status = false, message = "An error occurred while processing your request" });
            }
        }
    }
}