#pragma once

#include "Class.g.h"

namespace winrt::Component::implementation
{
    struct Class : ClassT<Class>
    {
        Class() = default;

        int32_t Int32Property() const
        {
            return m_int32;
        }
        void Int32Property(int32_t value)
        {
            m_int32 = value;
        }

        Windows::Foundation::IInspectable ObjectProperty() const
        {
            return m_object;
        }
        void ObjectProperty(Windows::Foundation::IInspectable const& value)
        {
            m_object = value;
        }

        int32_t m_int32{};
        Windows::Foundation::IInspectable m_object;
    };
}

namespace winrt::Component::factory_implementation
{
    struct Class : ClassT<Class, implementation::Class>
    {
    };
}
