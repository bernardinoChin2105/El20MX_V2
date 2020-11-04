using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Helpers;
using MVC_Project.Domain.Repositories;
using MVC_Project.Domain.Services;
using System;

using Unity;
using Unity.AspNet.Mvc;

namespace MVC_Project.WebBackend
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();
            container.RegisterType<IUnitOfWork, UnitOfWork>(new PerRequestLifetimeManager());
            container.RegisterType(typeof(IRepository<>), typeof(Repository<>));
            container.RegisterType(typeof(IService<>), typeof(ServiceBase<>));
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IPermissionService, PermissionService>();
            container.RegisterType<IRolePermissionService, RolePermissionService>();
            container.RegisterType<IRoleService, RoleService>();
            container.RegisterType<IAuthService, AuthService>();
            container.RegisterType<IPaymentService, PaymentService>();
            container.RegisterType<IEventService, EventService>();
            container.RegisterType<IOrderService, OrderService>();
            container.RegisterType<IProfileService, ProfileService>();
            container.RegisterType<IAccountService, AccountService>();
            container.RegisterType<IMembershipService, MembershipService>();
            container.RegisterType<ISocialNetworkLoginService, SocialNetworkLoginService>();
            container.RegisterType<IMembershipPermissionService, MembershipPermissionService>();
            container.RegisterType<IFeatureService, FeatureService>();
            container.RegisterType<ICredentialService, CredentialService>();
            container.RegisterType<IDiagnosticService, DiagnosticService>();
            container.RegisterType<ICustomerService, CustomerService>();
            container.RegisterType<ICustomerContactService, CustomerContactService>();
            container.RegisterType<IStateService, StateService>();
            container.RegisterType<IProviderService, ProviderService>();
            container.RegisterType<IBankService, BankService>();
            container.RegisterType<IBankCredentialService, BankCredentialService>();
            container.RegisterType<ICurrencyService, CurrencyService>();
            container.RegisterType<IBankAccountService, BankAccountService>();
            container.RegisterType<IPaymentFormService, PaymentFormService>();
            container.RegisterType<IPaymentMethodService, PaymentMethodService>();
            container.RegisterType<IInvoiceIssuedService, InvoiceIssuedService>();
            container.RegisterType<IInvoiceReceivedService, InvoiceReceivedService>();
            container.RegisterType<ICountryService, CountryService>();
            container.RegisterType<IPlanService, PlanService>();
            container.RegisterType<IPlanChargeService, PlanChargeService>();
            container.RegisterType<IPlanFeatureService, PlanFeatureService>();
            container.RegisterType<IPlanAssignmentsService, PlanAssignmentsService>();
            container.RegisterType<IPlanChargeConfigService, PlanChargeConfigService>();
            container.RegisterType<IPlanFeatureConfigService, PlanFeatureConfigService>();
            container.RegisterType<IPlanAssignmentConfigService, PlanAssignmentConfigService>();
            container.RegisterType<IAllianceService, AllianceService>();
            container.RegisterType<IAllyService, AllyService>();
            container.RegisterType<IPromotionService, PromotionService>();
            container.RegisterType<IPromotionAccountService, PromotionAccountService>();
            container.RegisterType<IDiscountService, DiscountService>();
            container.RegisterType<IQuotationService, QuotationService>();
            container.RegisterType<ICustomsService, CustomsService>();
            container.RegisterType<ICustomsPatentService, CustomsPatentService>();
            container.RegisterType<ICustomsRequestNumberService, CustomsRequestNumberService>();
            container.RegisterType<IDriveKeyService, DriveKeyService>();
            container.RegisterType<IProductServiceKeyService, ProductServiceKeyService>();
            container.RegisterType<ITaxService, TaxService>();
            container.RegisterType<ITaxRegimeService, TaxRegimeService>();
            container.RegisterType<ITypeRelationshipService, TypeRelationshipService>();
            container.RegisterType<IUseCFDIService, UseCFDIService>();
            container.RegisterType<ITypeInvoiceService, TypeInvoiceService>();
            container.RegisterType<ITypeVoucherService, TypeVoucherService>();
            container.RegisterType<IQuotationDetailService, QuotationDetailService>();
            container.RegisterType<IBranchOfficeService, BranchOfficeService>();
            container.RegisterType<ICADAccountService, CADAccountService>();
        }
    }
}