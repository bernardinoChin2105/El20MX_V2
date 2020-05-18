using System;
using System.Collections.Generic;
using System.Text;

namespace MVC_Project.Domain.Helpers {

    public interface IUnitOfWork {

        void BeginTransaction();

        void Commit();

        void Rollback();
    }
}